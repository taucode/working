namespace TauCode.Working.Labor;

[Flags]
public enum LaborerState
{
    Stopped = 0x00000001,
    Starting = 0x00000001 << 1,
    Running = 0x00000001 << 2,
    Stopping = 0x00000001 << 3,
    Pausing = 0x00000001 << 4,
    Paused = 0x00000001 << 5,
    Resuming = 0x00000001 << 6,
    Disposed = 0x00000001 << 7,
}