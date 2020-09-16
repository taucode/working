namespace TauCode.Working.ZetaOld.Workers
{
    public enum ZetaWorkerState
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
