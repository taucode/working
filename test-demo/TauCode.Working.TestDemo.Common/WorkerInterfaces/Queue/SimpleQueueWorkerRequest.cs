namespace TauCode.Working.TestDemo.Common.WorkerInterfaces.Queue
{
    public class SimpleQueueWorkerRequest
    {
        public int? From { get; set; }
        public int? To { get; set; }
        public int? WorkDelay { get; set; }
    }
}
