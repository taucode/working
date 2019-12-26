namespace TauCode.Working.Lab
{
    public interface IQueueWorker<in TAssignment> : IWorker
    {
        void Enqueue(TAssignment assignment);

        int Backlog { get; }
    }
}
