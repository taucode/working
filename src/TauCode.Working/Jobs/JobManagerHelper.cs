using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;

namespace TauCode.Working.Jobs
{
    // todo clean up
    internal class JobManagerHelper : LoopWorkerBase
    {
        #region Constants

        private const int VacationTimeoutMilliseconds = 10;
        private const int ScheduleChangedSignalIndex = 1;

        private static readonly TimeSpan InfiniteTimeSpan = TimeSpan.FromMilliseconds(int.MaxValue);
        private static readonly DateTime Never = new DateTime(9000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion

        #region Nested

        // todo: 'internal' in nested types, not 'public'.
        private class ScheduleEntry
        {
            public ScheduleEntry(string jobName, DateTime dueTime)
            {
                this.JobName = jobName;
                this.DueTime = dueTime;
            }

            public string JobName { get; }
            public DateTime DueTime { get; private set; }

            public void ChangeDueTime(DateTime dueTime)
            {
                this.DueTime = dueTime;
            }
        }

        #endregion

        #region Fields

        //private readonly Dictionary<string, ScheduleRegistration> _registrations;
        //private readonly SortedList<ScheduleKey, ScheduleRegistration> _list;

        private readonly JobManager _host;

        //private readonly List<ScheduleEntry> _list;

        private readonly Dictionary<string, ScheduleEntry> _entries;

        private AutoResetEvent _scheduleChangedEvent; // disposed by LoopWorkerBase.Shutdown
        private readonly object _scheduleLock;

        #endregion

        #region Constructor

        internal JobManagerHelper(JobManager host)
        {
            //_registrations = new Dictionary<string, ScheduleRegistration>();
            //_list = new SortedList<ScheduleKey, ScheduleRegistration>(Comparer<ScheduleKey>.Default);

            _host = host;
            //_list = new List<ScheduleEntry>();
            _entries = new Dictionary<string, ScheduleEntry>();

            _scheduleLock = new object();
        }

        #endregion

        #region Private

        //private int? GetIndexOfEntryWithClosestDueTime(out DateTime dueTime)
        //{
        //    lock (_scheduleLock)
        //    {
        //        var minTime = Never;
        //        var index = int.MaxValue;

        //        for (var i = 0; i < _list.Count; i++)
        //        {
        //            var entry = _list[i];
        //            if (entry.DueTime < minTime)
        //            {
        //                index = i;
        //                minTime = entry.DueTime;
        //            }
        //        }

        //        if (index == int.MaxValue)
        //        {
        //            dueTime = Never;
        //            return null;
        //        }

        //        dueTime = minTime;
        //        return index;
        //    }
        //}

        private ScheduleEntry GetClosestEntry()
        {
            lock (_scheduleLock)
            {
                if (_entries.Count == 0)
                {
                    return null;
                }

                var min = Never;
                ScheduleEntry bestEntry = null;

                foreach (var pair in _entries)
                {
                    var entry = pair.Value;
                    if (entry.DueTime < min)
                    {
                        bestEntry = entry;
                        min = entry.DueTime;
                    }
                }

                return bestEntry;
            }
        }

        #endregion

        #region Overridden

        protected override Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            string jobNameToStart = null;

            lock (_scheduleLock)
            {
                var entry = this.GetClosestEntry();

                if (entry == null)
                {
                    // nothing to work on.
                    return Task.FromResult(WorkFinishReason.WorkIsDone);
                }
                else
                {
                    var now = TimeProvider.GetCurrent();
                    var dueTime = entry.DueTime;

                    if (dueTime <= now)
                    {
                        // gotta run this job
                        jobNameToStart = entry.JobName;
                    }
                }
            }

            if (jobNameToStart != null)
            {
                // start the job
                Task.Run(() => _host.StartJob(jobNameToStart));

                // get schedule and re-schedule job.
                var jobSchedule = _host.GetSchedule(jobNameToStart);
                this.Reschedule(jobNameToStart, jobSchedule);
            }

            return Task.FromResult(WorkFinishReason.WorkIsDone);
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            TimeSpan vacationTimeout;

            lock (_scheduleLock)
            {
                var entry = this.GetClosestEntry();
                if (entry == null)
                {
                    vacationTimeout = InfiniteTimeSpan; // no candidates, let's party 'forever'
                }
                else
                {
                    var dueTime = entry.DueTime;
                    var now = TimeProvider.GetCurrent();
                    if (dueTime <= now)
                    {
                        // oh, we've got a due time, terminate vacation immediately!
                        return Task.FromResult(VacationFinishReason.VacationTimeElapsed);
                    }
                    else
                    {
                        // got some time to have fun before the due time
                        vacationTimeout = dueTime - now;
                        if (vacationTimeout > InfiniteTimeSpan)
                        {
                            vacationTimeout = InfiniteTimeSpan;
                        }
                    }
                }
            }

            var signalIndex = this.WaitForControlSignalWithExtraSignals(vacationTimeout);

            switch (signalIndex)
            {
                case ControlSignalIndex:
                    return Task.FromResult(VacationFinishReason.GotControlSignal);

                case ScheduleChangedSignalIndex:
                    return Task.FromResult(VacationFinishReason.NewWorkArrived);

                case WaitHandle.WaitTimeout:
                    // our vacation is over.
                    return Task.FromResult(VacationFinishReason.VacationTimeElapsed);

                default:
                    throw this.CreateInternalErrorException(); // should never happen
            }
        }

        protected override IList<AutoResetEvent> CreateExtraSignals()
        {
            _scheduleChangedEvent = new AutoResetEvent(false);
            return new[] { _scheduleChangedEvent };
        }

        #endregion

        #region Internal

        //internal void OnNewRegistration(ScheduleRegistration registration)
        //{
        //    // this method is always protected by lock ScheduleManager._lock, so no additional lock is needed.

        //    lock (_scheduleLock)
        //    {
        //        var now = TimeProvider.GetCurrent();
        //        if (now.Kind != DateTimeKind.Utc)
        //        {
        //            throw new NotImplementedException(); // todo wtf
        //        }

        //        var dueTime = registration.Schedule.GetDueTimeAfter(now);

        //        if (dueTime <= now)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        if (dueTime.Kind != DateTimeKind.Utc)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        if (dueTime.Millisecond != 0)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        //_registrations.Add(registration.RegistrationId, registration);
        //        //_list.Add(new ScheduleKey(dueTime, registration.RegistrationId), registration);

        //        var entry = new ScheduleEntry(registration, dueTime);
        //        _list.Add(entry);
        //        _scheduleChangedEvent.Set();
        //    }
        //}

        #endregion

        internal void Reschedule(string jobName, ISchedule jobSchedule)
        {
            // todo check args
            var now = TimeProvider.GetCurrent();

            if (now.Kind != DateTimeKind.Utc)
            {
                throw new NotImplementedException(); // todo wtf
            }

            var dueTime = jobSchedule.GetDueTimeAfter(now);
            Console.WriteLine($"DUE TIME: {dueTime.FormatTime()}");

            if (dueTime <= now)
            {
                throw new NotImplementedException();
            }

            if (dueTime.Kind != DateTimeKind.Utc)
            {
                throw new NotImplementedException();
            }

            //if (dueTime.Millisecond != 0)
            //{
            //    throw new NotImplementedException();
            //}

            lock (_scheduleLock)
            {
                // maybe we already have entry with that name
                var exists = _entries.TryGetValue(jobName, out var entry);
                if (!exists)
                {
                    entry = new ScheduleEntry(jobName, dueTime);
                    _entries.Add(entry.JobName, entry);
                }

                entry.ChangeDueTime(dueTime);

                //_registrations.Add(registration.RegistrationId, registration);
                //_list.Add(new ScheduleKey(dueTime, registration.RegistrationId), registration);

                //var entry = new ScheduleEntry(jobName, dueTime);
                //_list.Add(entry);
                //_scheduleChangedEvent.Set();
            }

            _scheduleChangedEvent.Set();
        }
    }
}
