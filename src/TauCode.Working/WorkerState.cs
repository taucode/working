namespace TauCode.Working.Lab
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
