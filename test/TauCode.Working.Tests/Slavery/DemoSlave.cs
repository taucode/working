using Serilog;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

public class DemoSlave : SlaveBase
{
    private readonly object _historyLock;
    private readonly List<SlaveState> _history;

    public DemoSlave(ILogger? logger)
        : base(logger)
    {
        _historyLock = new object();
        _history = new List<SlaveState>();

        AddStateToHistory();
    }

    public IReadOnlyList<SlaveState> History
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
            _history.Add(State);
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

        AddStateToHistory();
        Thread.Sleep(OnBeforeStartingTimeout);
        if (ThrowsOnBeforeStarting)
        {
            throw CreateFailException(nameof(OnBeforeStarting));
        }
    }

    protected override void OnStarting()
    {
        base.OnStarting();

        AddStateToHistory();
        Thread.Sleep(OnStartingTimeout);
        if (ThrowsOnStarting)
        {
            throw CreateFailException(nameof(OnStarting));
        }
    }

    protected override void OnAfterStarted()
    {
        base.OnAfterStarted();

        AddStateToHistory();
        Thread.Sleep(OnAfterStartedTimeout);
        if (ThrowsOnAfterStarted)
        {
            throw CreateFailException(nameof(OnAfterStarted));
        }
    }

    #endregion

    #region Stop-related

    protected override void OnBeforeStopping()
    {
        AddStateToHistory();
        Thread.Sleep(OnBeforeStoppingTimeout);
        if (ThrowsOnBeforeStopping)
        {
            throw CreateFailException(nameof(OnBeforeStopping));
        }
    }

    protected override void OnStopping()
    {
        base.OnStopping();

        AddStateToHistory();
        Thread.Sleep(OnStoppingTimeout);
        if (ThrowsOnStopping)
        {
            throw CreateFailException(nameof(OnStopping));
        }
    }

    protected override void OnAfterStopped()
    {
        AddStateToHistory();
        Thread.Sleep(OnAfterStoppedTimeout);
        if (ThrowsOnAfterStopped)
        {
            throw CreateFailException(nameof(OnAfterStopped));
        }
    }


    #endregion

    #region Pause-related

    protected override void OnBeforePausing()
    {
        AddStateToHistory();
        Thread.Sleep(OnBeforePausingTimeout);
        if (ThrowsOnBeforePausing)
        {
            throw CreateFailException(nameof(OnBeforePausing));
        }
    }

    protected override void OnPausing()
    {
        AddStateToHistory();
        Thread.Sleep(OnPausingTimeout);
        if (ThrowsOnPausing)
        {
            throw CreateFailException(nameof(OnPausing));
        }
    }

    protected override void OnAfterPaused()
    {
        AddStateToHistory();
        Thread.Sleep(OnAfterPausedTimeout);
        if (ThrowsOnAfterPaused)
        {
            throw CreateFailException(nameof(OnAfterPaused));
        }
    }

    #endregion

    #region Resume-related

    protected override void OnBeforeResuming()
    {
        AddStateToHistory();
        Thread.Sleep(OnBeforeResumingTimeout);
        if (ThrowsOnBeforeResuming)
        {
            throw CreateFailException(nameof(OnBeforeResuming));
        }
    }

    protected override void OnResuming()
    {
        AddStateToHistory();
        Thread.Sleep(OnResumingTimeout);
        if (ThrowsOnResuming)
        {
            throw CreateFailException(nameof(OnResuming));
        }
    }

    protected override void OnAfterResumed()
    {
        AddStateToHistory();
        Thread.Sleep(OnAfterResumedTimeout);
        if (ThrowsOnAfterResumed)
        {
            throw CreateFailException(nameof(OnAfterResumed));
        }
    }


    #endregion

    protected override void OnAfterDisposed()
    {
        Thread.Sleep(OnAfterDisposedTimeout);
        if (ThrowsOnAfterDisposed)
        {
            throw CreateFailException(nameof(OnAfterDisposed));
        }
    }

    #endregion

    private SystemException CreateFailException(string operationName)
    {
        throw new SystemException($"{operationName} failed!");
    }
}