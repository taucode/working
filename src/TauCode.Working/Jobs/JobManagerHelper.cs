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

        //private readonly struct ScheduleKey : IComparable<ScheduleKey>, IEquatable<ScheduleKey>
        //{
        //    public ScheduleKey(DateTime dueTime, string subscriptionId)
        //    {
        //        if (dueTime.Kind != DateTimeKind.Utc)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        if (dueTime.Millisecond != 0)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        this.DueTime = dueTime;
        //        this.SubscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
        //    }

        //    public DateTime DueTime { get; }
        //    public string SubscriptionId { get; }

        //    public int CompareTo(ScheduleKey other)
        //    {
        //        var dueTimeComparison = DueTime.CompareTo(other.DueTime);

        //        if (dueTimeComparison != 0)
        //        {
        //            return dueTimeComparison;
        //        }

        //        return string.Compare(SubscriptionId, other.SubscriptionId, StringComparison.Ordinal);
        //    }

        //    public bool Equals(ScheduleKey other)
        //    {
        //        return
        //            DueTime.Equals(other.DueTime) &&
        //            SubscriptionId == other.SubscriptionId;
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        return obj is ScheduleKey other && Equals(other);
        //    }

        //    public override int GetHashCode()
        //    {
        //        return HashCode.Combine(DueTime, SubscriptionId);
        //    }
        //}

        //private class ScheduleEntry
        //{
        //    public ScheduleEntry(ScheduleRegistration registration, DateTime dueTime)
        //    {
        //        // todo: checks on utc?

        //        this.Registration = registration;
        //        this.DueTime = dueTime;
        //    }

        //    public ScheduleRegistration Registration { get; }
        //    public DateTime DueTime { get; set; } // todo: use method
        //}

        private class ScheduleEntry
        {
            public ScheduleEntry(string jobName, DateTime dueTime)
            {
                this.JobName = jobName;
                this.DueTime = dueTime;
            }

            public string JobName { get; }
            public DateTime DueTime { get; }
        }

        #endregion

        #region Fields

        //private readonly Dictionary<string, ScheduleRegistration> _registrations;
        //private readonly SortedList<ScheduleKey, ScheduleRegistration> _list;

        private readonly JobManager _host;

        private readonly List<ScheduleEntry> _list;

        private AutoResetEvent _scheduleChangedEvent; // disposed by LoopWorkerBase.Shutdown
        private readonly object _scheduleLock;

        #endregion

        #region Constructor

        internal JobManagerHelper(JobManager host)
        {
            //_registrations = new Dictionary<string, ScheduleRegistration>();
            //_list = new SortedList<ScheduleKey, ScheduleRegistration>(Comparer<ScheduleKey>.Default);

            _host = host;
            _list = new List<ScheduleEntry>();

            _scheduleLock = new object();
        }

        #endregion

        #region Private

        private int? GetIndexOfEntryWithClosestDueTime(out DateTime dueTime)
        {
            lock (_scheduleLock)
            {
                var minTime = Never;
                var index = int.MaxValue;

                for (var i = 0; i < _list.Count; i++)
                {
                    var entry = _list[i];
                    if (entry.DueTime < minTime)
                    {
                        index = i;
                        minTime = entry.DueTime;
                    }
                }

                if (index == int.MaxValue)
                {
                    dueTime = Never;
                    return null;
                }

                dueTime = minTime;
                return index;
            }
        }

        #endregion

        #region Overridden

        protected override Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            lock (_scheduleLock)
            {
                var index = this.GetIndexOfEntryWithClosestDueTime(out var dueTime);

                if (index.HasValue)
                {
                    var now = TimeProvider.GetCurrent();
                    var entry = _list[index.Value];

                    if (dueTime <= now)
                    {
                        _host.StartJob(entry.JobName);
                        _list.RemoveAt(index.Value);

                        // todo: ugly.
                        this.OnNewRegistration(entry.JobName, _host.GetSchedule(entry.JobName));
                    }

                    //return Task.FromResult(WorkFinishReason.WorkIsDone);
                }

                //else
                //{
                //    return Task.FromResult(WorkFinishReason.WorkIsDone); // no candidates.
                //}

                return Task.FromResult(WorkFinishReason.WorkIsDone);

                //var tuple = this.GetClosestDueTime();
                //if (tuple == null)
                //{
                //    return Task.FromResult(WorkFinishReason.WorkIsDone); // no candidates.
                //}



                //if (tuple.Item2 <= now)
                //{
                //    var entry = _list[tuple.Item1];

                //    try
                //    {
                //        entry.Registration.Worker.Start();
                //        var nextDueTime = entry.Registration.Schedule.GetDueTimeAfter(now);
                //        entry.DueTime = nextDueTime;

                //        return Task.FromResult(WorkFinishReason.WorkIsDone); // let's have a rest.
                //    }
                //    catch (Exception e)
                //    {
                //        // todo
                //        Console.WriteLine(e);
                //        throw;
                //    }
                //}
                //else
                //{
                //    // due time not occurred yet.
                //    return Task.FromResult(WorkFinishReason.WorkIsDone);
                //}
            }
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            TimeSpan vacationTimeout;

            lock (_scheduleLock)
            {
                var index = this.GetIndexOfEntryWithClosestDueTime(out var dueTime);
                if (index.HasValue)
                {
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
                else
                {
                    vacationTimeout = InfiniteTimeSpan; // no candidates, let's party 'forever'
                }

                //var tuple = this.GetClosestDueTime();
                //if (tuple == null)
                //{
                //    vacationTimeout = InfiniteTimeSpan; // no candidates, let's party 'forever'
                //}
                //else
                //{
                //    var now = TimeProvider.GetCurrent();
                //    if (tuple.Item2 <= now)
                //    {
                //        // oh, we've got a due time, terminate vacation immediately!
                //        return Task.FromResult(VacationFinishReason.VacationTimeElapsed);
                //    }
                //    else
                //    {
                //        // got some time to have fun before the due time
                //        vacationTimeout = tuple.Item2 - now;
                //    }
                //}
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

        internal void OnNewRegistration(string jobName, ISchedule jobSchedule)
        {
            lock (_scheduleLock)
            {
                var now = TimeProvider.GetCurrent();
                if (now.Kind != DateTimeKind.Utc)
                {
                    throw new NotImplementedException(); // todo wtf
                }

                var dueTime = jobSchedule.GetDueTimeAfter(now);

                if (dueTime <= now)
                {
                    throw new NotImplementedException();
                }

                if (dueTime.Kind != DateTimeKind.Utc)
                {
                    throw new NotImplementedException();
                }

                if (dueTime.Millisecond != 0)
                {
                    throw new NotImplementedException();
                }

                //_registrations.Add(registration.RegistrationId, registration);
                //_list.Add(new ScheduleKey(dueTime, registration.RegistrationId), registration);

                var entry = new ScheduleEntry(jobName, dueTime);
                _list.Add(entry);
                _scheduleChangedEvent.Set();
            }
        }
    }
}
