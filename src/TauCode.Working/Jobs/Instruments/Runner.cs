using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Working.Exceptions;

// todo clean
namespace TauCode.Working.Jobs.Instruments
{
    internal class Runner : IDisposable
    {
        #region Fields

        private bool _isEnabled;
        private bool _isDisposed;

        //private CancellationTokenSource _tokenSource;
        //private StringWriterWithEncoding _systemWriter;

        //private Task _task;
        //private Task _endTask;

        private RunContext _runContext;

        private readonly object _lock;

        #endregion

        #region Constructor

        internal Runner(string name)
        {
            this.Name = name;

            this.JobPropertiesHolder = new JobPropertiesHolder();
            this.DueTimeHolder = new DueTimeHolder();
            this.JobRunsHolder = new JobRunsHolder();

            _lock = new object();
        }

        #endregion

        #region Private

        private string Name { get; }

        private void CheckNotDisposed()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new JobObjectDisposedException(this.Name);
                }
            }
        }

        private Task CreateTask(CancellationToken? token)
        {
            throw new NotImplementedException();
        }

        private void EndTask(Task task)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal

        internal bool IsEnabled
        {
            get
            {
                lock (_lock)
                {
                    return _isEnabled;
                }
            }
            set
            {
                lock (_lock)
                {
                    this.CheckNotDisposed();
                    _isEnabled = value;
                }
            }
        }

        internal bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _runContext != null;
                }
            }
        }

        private RunContext Run(JobStartReason startReason, CancellationToken? token)
        {
            // always guarded by '_lock'
            //var jobProperties = this.JobPropertiesHolder.ToJobProperties();
            var runContext = new RunContext(this, startReason, token);
            runContext.Start();
            return runContext;



            //_task = this.CreateTask(token);
            //_endTask = _task.ContinueWith(
            //    this.EndTask,
            //    CancellationToken.None);
        }


        internal JobPropertiesHolder JobPropertiesHolder { get; }

        internal DueTimeHolder DueTimeHolder { get; }

        internal JobRunsHolder JobRunsHolder { get; }

        internal bool IsDisposed
        {
            get
            {
                lock (_lock)
                {
                    return _isDisposed;
                }
            }
        }


        #endregion

        #region IDisposable

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                try
                {
                    _runContext?.Cancel();
                    _runContext = null;
                }
                catch
                {
                    // dismiss; Dispose shouldn't throw
                }

                this.JobPropertiesHolder.Dispose();
                this.DueTimeHolder.Dispose();

                //try
                //{
                //    _tokenSource?.Dispose();
                //}
                //catch
                //{
                //    // dismiss; Dispose shouldn't throw
                //}

                //try
                //{
                //    _systemWriter?.Dispose();
                //}
                //catch
                //{
                //    // dismiss; Dispose shouldn't throw
                //}

                //_isDisposed = true;
            }
        }

        #endregion

        internal DueTimeInfo? GetDueTimeInfoForVice(bool future)
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return null;
                }

                if (future)
                {
                    this.DueTimeHolder.UpdateScheduleDueTime();
                }

                return this.DueTimeHolder.GetDueTimeInfo();
            }
        }

        internal bool Cancel()
        {
            lock (_lock)
            {
                if (_runContext == null)
                {
                    return false;
                }

                _runContext.Cancel();
                return true;
            }
        }

        internal bool WakeUp(JobStartReason startReason, CancellationToken? token)
        {
            if (startReason == JobStartReason.Force2)
            {
                throw new NotImplementedException();
            }
            else
            {
                lock (_lock)
                {
                    if (this.IsRunning)
                    {
                        return false;
                    }

                    if (!this.IsEnabled)
                    {
                        this.DueTimeHolder.UpdateScheduleDueTime();
                        return false;
                    }

                    _runContext = this.Run(startReason, token);
                    return true;
                }
            }
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            var tuple = this.JobRunsHolder.Get();
            var currentRun = tuple.Item1;
            var runs = tuple.Item2;

            var dueTimeInfo = this.DueTimeHolder.GetDueTimeInfo();

            return new JobInfo(
                currentRun,
                dueTimeInfo.GetEffectiveDueTime(),
                dueTimeInfo.IsDueTimeOverridden(),
                this.JobRunsHolder.Count,
                runs);
        }

        internal void OnTaskEnded()
        {
            lock (_lock)
            {
                _runContext = null;
            }
        }
    }
}
