namespace TauCode.Working.Slavery;

public interface ISlave : IDisposable
{
    string? Name { get; set; }
    SlaveState State { get; }
    void Start();
    void Stop();
    void Pause();
    void Resume();
    bool IsDisposed { get; }
}