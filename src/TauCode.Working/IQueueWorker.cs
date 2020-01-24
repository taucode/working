namespace TauCode.Working
{
    public interface IQueueWorker<in TAssignment> : IWorker
    {
        void Enqueue(TAssignment assignment);

        int Backlog { get; }
    }
}
