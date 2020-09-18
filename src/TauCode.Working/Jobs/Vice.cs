using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs
{
    internal class Vice : LoopWorkerBase
    {
        #region Fields

        private readonly Dictionary<string, Employee> _employees;
        private readonly object _lock;

        private readonly ObjectLogger _logger;

        #endregion

        #region Constructor

        internal Vice()
        {
            _employees = new Dictionary<string, Employee>();
            _lock = new object();

            _logger = new ObjectLogger(this, null);
        }

        #endregion

        #region Overridden

        protected override Task<TimeSpan> DoWork(CancellationToken token)
        {
            _logger.Debug("Entered method", nameof(DoWork));

            var now = TimeProvider.GetCurrent();

            var employeesToWakeUp = new List<Tuple<Employee, DueTimeInfo>>();

            var earliest = JobExtensions.Never;

            lock (_lock)
            {
                foreach (var employee in _employees.Values)
                {
                    var info = employee.GetDueTimeInfoForVice(false);

                    if (!info.HasValue)
                    {
                        continue;
                    }

                    var dueTime = info.Value.GetEffectiveDueTime();

                    if (now >= dueTime)
                    {
                        // due time has come!
                        employeesToWakeUp.Add(Tuple.Create(employee, info.Value));
                    }
                    else
                    {
                        earliest = DateTimeExtensionsLab.Min(earliest, dueTime);
                    }
                }
            }

            foreach (var tuple in employeesToWakeUp)
            {
                // todo: log on exception
                var employee = tuple.Item1;
                var isOverridden = tuple.Item2.IsDueTimeOverridden();
                var reason = isOverridden ? JobStartReason.OverriddenDueTime : JobStartReason.ScheduleDueTime;

                var workStarted = employee.WakeUp(reason, token);

                // when to visit you again, Employee?
                var nextDueTimeInfo = employee.GetDueTimeInfoForVice(true);

                if (nextDueTimeInfo.HasValue) // actually, should have, he could not finish work and got disposed that fast, but who knows...
                {
                    var nextDueTime = nextDueTimeInfo.Value.GetEffectiveDueTime();
                    if (nextDueTime > now)
                    {
                        earliest = DateTimeExtensionsLab.Min(earliest, nextDueTime);
                    }
                }
            }

            var vacationTimeout = earliest - now;
            return Task.FromResult(vacationTimeout);
        }

        protected override void OnDisposed()
        {
            IList<Employee> list;
            lock (_lock)
            {
                list = _employees.Values.ToList();
            }

            foreach (var employee in list)
            {
                employee.Dispose();
            }
        }

        #endregion

        #region Internal

        internal IJob CreateJob(string jobName)
        {
            lock (_lock)
            {
                if (_employees.ContainsKey(jobName))
                {
                    throw new InvalidJobOperationException($"Job '{jobName}' already exists.");
                }

                var employee = new Employee(this, jobName);
                if (this.IsLoggingEnabled)
                {
                    employee.EnableLogging(true);
                }

                _employees.Add(employee.Name, employee);

                this.WorkArrived();
                return employee.GetJob();
            }
        }

        internal IJob GetJob(string jobName)
        {
            lock (_lock)
            {
                var employee = _employees.GetValueOrDefault(jobName);
                if (employee == null)
                {
                    throw new InvalidJobOperationException($"Job not found: '{jobName}'.");
                }

                return employee.GetJob();
            }
        }

        internal IReadOnlyList<string> GetJobNames()
        {
            lock (_lock)
            {
                return _employees.Keys.ToList();
            }
        }

        internal void PulseWork() => this.WorkArrived();

        internal bool IsLoggingEnabled => _logger.IsEnabled;

        internal void EnableLogging(bool enable)
        {
            _logger.IsEnabled = enable;
            lock (_logger)
            {
                foreach (var employee in _employees.Values)
                {
                    employee.EnableLogging(enable);
                }
            }
        }

        #endregion
    }
}
