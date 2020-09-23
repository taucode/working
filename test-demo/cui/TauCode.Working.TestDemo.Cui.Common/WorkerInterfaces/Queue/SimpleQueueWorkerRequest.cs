namespace TauCode.Working.TestDemo.Cui.Common.WorkerInterfaces.Queue
{
    public class SimpleQueueWorkerRequest
    {
        public int? From { get; set; }
        public int? To { get; set; }
        public int? JobDelay { get; set; }
    }
}
