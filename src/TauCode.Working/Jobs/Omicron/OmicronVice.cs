using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Labor;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronVice : CycleProlBase
    {
        private readonly Dictionary<string, OmicronEmployee> _employees;
        private readonly object _lock;

        internal OmicronVice()
        {
            _employees = new Dictionary<string, OmicronEmployee>();
            _lock = new object();
        }

        protected override Task<TimeSpan> DoWork(CancellationToken token)
        {
            var now = TimeProvider.GetCurrent();
            //var employeesToWakeUp = new List<OmicronEmployee>();

            var employeesToWakeUp = new List<Tuple<OmicronEmployee, DueTimeInfoForVice>>();

            var earliest = JobExtensions.Never;

            lock (_lock)
            {
                foreach (var employee in _employees.Values)
                {
                    var info = employee.GetDueTimeInfoForVice();

                    if (!info.HasValue)
                    {
                        continue;
                    }



                    //var dueTime = employee.GetDueTimeForVice();
                    //if (!dueTime.HasValue)
                    //{
                    //    continue;
                    //}

                    if (now >= info.Value.DueTime)
                    {
                        // due time has come!
                        employeesToWakeUp.Add(Tuple.Create(employee, info.Value));
                    }
                    else
                    {
                        earliest = DateTimeExtensionsLab.Min(earliest, info.Value.DueTime);
                    }
                }
            }

            foreach (var tuple in employeesToWakeUp)
            {
                // todo: log on exception
                //employee.WakeUp(token); // todo: log if already was started etc

                var employee = tuple.Item1;
                var isOverridden = tuple.Item2.IsOverridden;
                var reason = isOverridden ? JobStartReason.OverriddenDueTime : JobStartReason.ScheduleDueTime;

                employee.WakeUp(reason, token);
            }

            var vacationTimeout = earliest - now;
            return Task.FromResult(vacationTimeout);
        }

        internal IJob CreateJob(string jobName)
        {
            lock (_lock)
            {
                if (_employees.ContainsKey(jobName))
                {
                    throw new InvalidJobOperationException($"Job '{jobName}' already exists.");
                }

                var employee = new OmicronEmployee(this, jobName);
                //{
                //    Name = jobName,
                //};

                _employees.Add(employee.Name, employee);
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

        protected override void OnDisposed()
        {
            IList<OmicronEmployee> list;
            lock (_lock)
            {
                list = _employees.Values.ToList();
            }

            foreach (var employee in list)
            {
                employee.Dispose();
            }
        }
    }
}
