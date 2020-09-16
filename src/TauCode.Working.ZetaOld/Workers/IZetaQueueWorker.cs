namespace TauCode.Working.ZetaOld.Workers
{
    public interface IZetaQueueWorker<in TAssignment> : IZetaWorker
    {
        void Enqueue(TAssignment assignment);

        int Backlog { get; }
    }
}
