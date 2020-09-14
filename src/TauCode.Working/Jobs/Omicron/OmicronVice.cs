using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            return Task.FromResult(TimeSpan.MaxValue); // todo0
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
    }
}
