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

    #region Start-related test props

    public bool ThrowsOnBeforeStarting { get; set; }
    public TimeSpan OnBeforeStartingTimeout { get; set; }

    public bool ThrowsOnStarting { get; set; }
    public TimeSpan OnStartingTimeout { get; set; }

    public bool ThrowsOnAfterStarted { get; set; }
    public TimeSpan OnAfterStartedTimeout { get; set; }

    #endregion

    #region Stop-related test props

    public bool ThrowsOnBeforeStopping { get; set; }
    public TimeSpan OnBeforeStoppingTimeout { get; set; }

    public bool ThrowsOnStopping { get; set; }
    public TimeSpan OnStoppingTimeout { get; set; }

    public bool ThrowsOnAfterStopped { get; set; }
    public TimeSpan OnAfterStoppedTimeout { get; set; }

    #endregion

    #region Pause-related test props

    public bool ThrowsOnBeforePausing { get; set; }
    public TimeSpan OnBeforePausingTimeout { get; set; }

    public bool ThrowsOnPausing { get; set; }
    public TimeSpan OnPausingTimeout { get; set; }

    public bool ThrowsOnAfterPaused { get; set; }
    public TimeSpan OnAfterPausedTimeout { get; set; }

    #endregion

    #region Resume-related test props

    public bool ThrowsOnBeforeResuming { get; set; }
    public TimeSpan OnBeforeResumingTimeout { get; set; }

    public bool ThrowsOnResuming { get; set; }
    public TimeSpan OnResumingTimeout { get; set; }

    public bool ThrowsOnAfterResumed { get; set; }
    public TimeSpan OnAfterResumedTimeout { get; set; }

    #endregion

    #region Dispose-related test props

    public bool ThrowsOnAfterDisposed { get; set; }
    public TimeSpan OnAfterDisposedTimeout { get; set; }

    #endregion

    #region Overridden

    #region Start-related

    protected override void OnBeforeStarting()
    {
        base.OnBeforeStarting();

        this.AddStateToHistory();
        Thread.Sleep(this.OnBeforeStartingTimeout);
        if (this.ThrowsOnBeforeStarting)
        {
            throw this.CreateFailException(nameof(OnBeforeStarting));
        }
    }

    protected override void OnStarting()
    {
        base.OnStarting();

        this.AddStateToHistory();
        Thread.Sleep(this.OnStartingTimeout);
        if (this.ThrowsOnStarting)
        {
            throw this.CreateFailException(nameof(OnStarting));
        }
    }

    protected override void OnAfterStarted()
    {
        base.OnAfterStarted();

        this.AddStateToHistory();
        Thread.Sleep(this.OnAfterStartedTimeout);
        if (this.ThrowsOnAfterStarted)
        {
            throw this.CreateFailException(nameof(OnAfterStarted));
        }
    }

    #endregion

    #region Stop-related

    protected override void OnBeforeStopping()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnBeforeStoppingTimeout);
        if (this.ThrowsOnBeforeStopping)
        {
            throw this.CreateFailException(nameof(OnBeforeStopping));
        }
    }

    protected override void OnStopping()
    {
        base.OnStopping();

        this.AddStateToHistory();
        Thread.Sleep(this.OnStoppingTimeout);
        if (this.ThrowsOnStopping)
        {
            throw this.CreateFailException(nameof(OnStopping));
        }
    }

    protected override void OnAfterStopped()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnAfterStoppedTimeout);
        if (this.ThrowsOnAfterStopped)
        {
            throw this.CreateFailException(nameof(OnAfterStopped));
        }
    }


    #endregion

    #region Pause-related

    protected override void OnBeforePausing()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnBeforePausingTimeout);
        if (this.ThrowsOnBeforePausing)
        {
            throw this.CreateFailException(nameof(OnBeforePausing));
        }
    }

    protected override void OnPausing()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnPausingTimeout);
        if (this.ThrowsOnPausing)
        {
            throw this.CreateFailException(nameof(OnPausing));
        }
    }

    protected override void OnAfterPaused()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnAfterPausedTimeout);
        if (this.ThrowsOnAfterPaused)
        {
            throw this.CreateFailException(nameof(OnAfterPaused));
        }
    }

    #endregion

    #region Resume-related

    protected override void OnBeforeResuming()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnBeforeResumingTimeout);
        if (this.ThrowsOnBeforeResuming)
        {
            throw this.CreateFailException(nameof(OnBeforeResuming));
        }
    }

    protected override void OnResuming()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnResumingTimeout);
        if (this.ThrowsOnResuming)
        {
            throw this.CreateFailException(nameof(OnResuming));
        }
    }

    protected override void OnAfterResumed()
    {
        this.AddStateToHistory();
        Thread.Sleep(this.OnAfterResumedTimeout);
        if (this.ThrowsOnAfterResumed)
        {
            throw this.CreateFailException(nameof(OnAfterResumed));
        }
    }


    #endregion

    protected override void OnAfterDisposed()
    {
        Thread.Sleep(this.OnAfterDisposedTimeout);
        if (this.ThrowsOnAfterDisposed)
        {
            throw this.CreateFailException(nameof(OnAfterDisposed));
        }
    }

    #endregion

    private SystemException CreateFailException(string operationName)
    {
        throw new SystemException($"{operationName} failed!");
    }
}