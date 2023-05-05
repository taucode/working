namespace TauCode.Working.Labor;

public interface ILaborer : IDisposable, IAsyncDisposable
{
    string? Name { get; set; }
    LaborerState State { get; }

    void Start();
    Task StartAsync(CancellationToken cancellationToken);

    void Stop();
    Task StopAsync(CancellationToken cancellationToken);

    void Pause();
    Task PauseAsync(CancellationToken cancellationToken);

    void Resume();
    Task ResumeAsync(CancellationToken cancellationToken);
}