//using System.Threading;
//using System.Threading.Tasks;

// todo clean
//namespace TauCode.Labor
//{
//    public abstract class TaskProlBase : ProlBase
//    {
//        private Task _currentTask;
//        private readonly object _runLock;

//        protected TaskProlBase()
//        {
//            _runLock = new object();
//        }

//        protected abstract Task CreateTask();

//        public bool RunTask()
//        {
//            var gotLock = Monitor.TryEnter(_runLock);
//            if (!gotLock)
//            {
//                return false;
//            }


//        }

//        protected override void OnStarting()
//        {
//            _currentTask = this.CreateTask(); // todo: checks for exceptions etc
//            _currentTask.ContinueWith(this.EndTask);
//        }
//    }
//}
