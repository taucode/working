using Serilog;

namespace TauCode.Working.Tests;

public class DemoWorker : WorkerBase
{
    private readonly object _historyLock;
    private readonly List<WorkerState> _history;

    public DemoWorker(ILogger? logger)
        : base(logger)
    {
        _historyLock = new object();
        _history = new List<WorkerState>();

        this.AddStateToHistory();
    }

    public IReadOnlyList<WorkerState> History
    {
        get
        {
            lock (_historyLock)
            {
                return _history.ToList();
            }
        }
    }

    private void AddStateToHistory()
    {
        lock (_historyLock)
        {
            _history.Add(this.State);
        }
    }

    public override bool IsPausingSupported => true;

    public TimeSpan OnStartingTimeout { get; set; }
    public TimeSpan OnStartedTimeout { get; set; }

    public TimeSpan OnStoppingTimeout { get; set; }
    public TimeSpan OnStoppedTimeout { get; set; }

    public TimeSpan OnPausingTimeout { get; set; }
    public TimeSpan OnPausedTimeout { get; set; }

    public TimeSpan OnResumingTimeout { get; set; }
    public TimeSpan OnResumedTimeout { get; set; }

    public TimeSpan OnDisposedTimeout { get; set; }

    protected override void OnBeforeStarting()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnStartingTimeout);
    }

    protected override void OnAfterStarted()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnStartedTimeout);
    }

    protected override void OnBeforeStopping()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnStoppingTimeout);
    }

    protected override void OnAfterStopped()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnStoppedTimeout);
    }

    protected override void OnBeforePausing()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnPausingTimeout);
    }

    protected override void OnAfterPaused()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnPausedTimeout);
    }

    protected override void OnBeforeResuming()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnResumingTimeout);
    }

    protected override void OnAfterResumed()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnResumedTimeout);
    }

    protected override void OnAfterDisposed()
    {
        Thread.Sleep(this.OnDisposedTimeout);
    }
}