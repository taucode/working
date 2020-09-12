namespace TauCode.Working.Workers
{
    public enum WorkerState
    {
        Stopping = 1,
        Stopped,

        Starting,
        Running,

        Pausing,
        Paused,

        Resuming,

        Disposing,
        Disposed,
    }
}
