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
            var employeesToWakeUp = new List<OmicronEmployee>();
            var earliest = JobExtensions.Never;

            lock (_lock)
            {
                foreach (var employee in _employees.Values)
                {
                    var dueTime = employee.GetDueTimeForVice();
                    if (!dueTime.HasValue)
                    {
                        continue;
                    }

                    if (now >= dueTime.Value)
                    {
                        // due time has come!
                        employeesToWakeUp.Add(employee);
                    }
                    else
                    {
                        earliest = DateTimeExtensionsLab.Min(earliest, dueTime.Value);
                    }
                }
            }

            foreach (var employee in employeesToWakeUp)
            {
                // todo: log on exception
                employee.WakeUp(token); // todo: log if already was started etc
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

                var employee = new OmicronEmployee(this)
                {
                    Name = jobName,
                };

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

        internal void OnScheduleChanged() => this.WorkArrived();

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
