//using System;

// todo clean up

//namespace TauCode.Working.Scheduling
//{
//    public class ScheduledWorkerBase : OnDemandWorkerBase, IScheduledWorker
//    {
//        #region Fields

//        private ISchedule _schedule;
//        private readonly object _lock;

//        #endregion

//        #region Constructor

//        public ScheduledWorkerBase(ISchedule schedule)
//        {
//            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
//            _lock = new object();
//        }

//        #endregion

//        #region IScheduledWorker Members

//        public ISchedule Schedule
//        {
//            get
//            {
//                lock (_lock)
//                {
//                    return _schedule;
//                }
//            }
//            set
//            {
//                lock (_lock)
//                {
//                    _schedule = value ?? throw new ArgumentNullException(nameof(value));
//                }

//                this.ScheduleChanged?.Invoke(new ScheduleChangedEventArgs(this));
//            }
//        }

//        public event Action<ScheduleChangedEventArgs> ScheduleChanged;

//        #endregion
//    }
//}
