using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;

namespace TauCode.Labor
{
    public abstract class CycleProlBase : ProlBase
    {
        protected readonly TimeSpan VeryLongVacation = TimeSpan.FromMilliseconds(int.MaxValue);
        protected readonly TimeSpan TimeQuantum = TimeSpan.FromMilliseconds(1);

        private Thread _thread;

        protected void NewWorkArrived()
        {
            lock (this.Lock)
            {
                Monitor.Pulse(this.Lock);
            }
        }

        protected override void OnStarting()
        {
            // todo: check '_thread' is null
            _thread = new Thread(CycleRoutine);
            _thread.Start();

            Monitor.Wait(this.Lock);
        }

        protected override void OnStopping()
        {
            lock (this.Lock)
            {
                Monitor.Pulse(this.Lock);
            }

            _thread.Join();
            _thread = null;
        }

        protected abstract Task<TimeSpan> DoWork(CancellationToken token);

        private void CycleRoutine()
        {
            lock (this.Lock)
            {
                Monitor.Pulse(this.Lock);
            }

            var source = new CancellationTokenSource();
            var taskEndedSignal = new ManualResetEventSlim(true);

            while (true)
            {
                var vacation = VeryLongVacation;

                if (taskEndedSignal.IsSet)
                {
                    // can try do some work.
                    var task = this.DoWork(source.Token); // todo: try/catch, not null etc.

                    if (task.IsCompleted)
                    {
                        // todo: log warning if task status is not 'RanToCompletion'
                        var wantedVacation = task.Result;
                        vacation = DateTimeExtensionsLab.MinMax(
                            TimeQuantum,
                            VeryLongVacation,
                            wantedVacation);
                    }
                    else
                    {
                        // task is not ended yet
                        taskEndedSignal.Reset();
                        task.ContinueWith(this.EndWork, taskEndedSignal, source.Token);
                    }
                }

                lock (this.Lock)
                {
                    Monitor.Wait(this.Lock, vacation);
                }

                if (this.State != ProlState.Running)
                {
                    source.Cancel();
                    break;
                }
            }

            taskEndedSignal.Wait();

            source.Dispose();
            taskEndedSignal.Dispose();
        }

        private void EndWork(Task initialTask, object taskEndedSignalObject)
        {
            var taskEndedSignal = (ManualResetEventSlim)taskEndedSignalObject;
            taskEndedSignal.Set();

            lock (this.Lock)
            {
                Monitor.Pulse(this.Lock);
            }
        }
    }
}
