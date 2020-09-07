namespace TauCode.Working.TestDemo.Common.WorkerInterfaces.Queue
{
    public class SimpleQueueWorkerResponse
    {
        public int? Backlog { get; set; }
        public int? JobDelay { get; set; }
        public ExceptionInfo Exception { get; set; }
    }
}
