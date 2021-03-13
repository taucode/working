using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;

// todo clean
namespace TauCode.Working
{
    public abstract class LoopWorkerBase : WorkerBase
    {
        #region Constants

        protected readonly TimeSpan VeryLongVacation = TimeSpan.FromMilliseconds(int.MaxValue);
        protected readonly TimeSpan TimeQuantum = TimeSpan.FromMilliseconds(1);

        #endregion

        #region Fields

        private readonly object _runningLock;
        private readonly object _startingLock;
        private readonly object _threadLock;

        private Thread _thread;
        

        private long _workGeneration; // increments each time new work arrived, or existing work is completed.



        //private bool _debugIsInVacation; // todo
        //private TimeSpan? _debugVacationLength; // todo
        //private bool _debugThreadExited; // todo

        #endregion

        #region Constructor

        protected LoopWorkerBase()
        {
            _runningLock = new object();
            _startingLock = new object();
            _threadLock = new object();
        }

        #endregion

        #region Abstract

        protected abstract Task<TimeSpan> DoWork(CancellationToken cancellationToken);

        #endregion

        #region Overridden

        protected override void OnStarting()
        {
            // todo: check '_thread' is null
            _thread = new Thread(LoopRoutine);

            lock (_startingLock)
            {
                _thread.Start();
                Monitor.Wait(_startingLock);
            }
        }

        protected override void OnStarted()
        {
            lock (_runningLock)
            {
                Monitor.Pulse(_runningLock);
            }
        }

        protected override void OnStopping()
        {
            lock (_threadLock)
            {
                Monitor.Pulse(_threadLock);
            }

            _thread.Join();
            _thread = null;
        }

        #endregion

        #region Protected

        protected void AdvanceWorkGeneration()
        {
            lock (_threadLock)
            {
                _workGeneration++;
                Monitor.Pulse(_threadLock);
            }
        }

        protected long GetCurrentWorkGeneration()
        {
            lock (_threadLock)
            {
                return _workGeneration;
            }
        }

        #endregion

        #region Private

        private void LoopRoutine()
        {
            lock (_runningLock)
            {
                lock (_startingLock)
                {
                    Monitor.Pulse(_startingLock);
                }

                Monitor.Wait(_runningLock);
            }

            var source = new CancellationTokenSource();
            var endTask = Task.CompletedTask;

            while (true)
            {
                var vacation = VeryLongVacation;

                if (endTask.IsCompleted)
                {
                    // can try do some work.
                    var task = this.DoWork(source.Token); // todo: try/catch, not null etc.

                    if (task.IsCompleted) // todo: other statuses?
                    {
                        // todo: log warning if task status is not 'RanToCompletion'
                        var wantedVacation = task.Result;
                        vacation = TimeSpanExtensions.MinMax(
                            TimeQuantum,
                            VeryLongVacation,
                            wantedVacation);
                    }
                    else
                    {
                        // task is not ended yet
                        endTask = task.ContinueWith(this.EndWork, source.Token, source.Token);
                    }
                }

                // todo: what if 'endTask' ended here? or WorkGeneration changed 'tipa'?
                var generationBeforeVacation = this.GetCurrentWorkGeneration();

                lock (_threadLock)
                {
                    var state = this.State;
                    if (state != WorkerState.Running)
                    {
                        break;
                    }

                    var generationRightAfterVacationStarted = this.GetCurrentWorkGeneration();
                    if (generationBeforeVacation != generationRightAfterVacationStarted)
                    {
                        // vacation is terminated, let's get back to work :(
                        continue;
                    }

                    //_debugIsInVacation = true;
                    //_debugVacationLength = vacation;

                    Monitor.Wait(_threadLock, vacation);

                    //_debugIsInVacation = false;
                    //_debugVacationLength = null;
                }

                var state2 = this.State;
                if (state2 != WorkerState.Running)
                {
                    break;
                }
            }

            source.Cancel();
            endTask.Wait();
            source.Dispose();

            //_debugThreadExited = true;
        }

        private void EndWork(Task initialTask, object state)
        {
            this.AdvanceWorkGeneration();
        }

        #endregion
    }
}
