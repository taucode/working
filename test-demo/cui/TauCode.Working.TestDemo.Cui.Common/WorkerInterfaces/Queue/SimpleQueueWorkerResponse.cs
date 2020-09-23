namespace TauCode.Working.TestDemo.Cui.Common.WorkerInterfaces.Queue
{
    public class SimpleQueueWorkerResponse
    {
        public int? Backlog { get; set; }
        public int? JobDelay { get; set; }
        public ExceptionInfo Exception { get; set; }
    }
}
