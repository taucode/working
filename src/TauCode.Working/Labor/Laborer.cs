using Serilog;
using TauCode.Infrastructure.Logging;

namespace TauCode.Working.Labor;

public abstract class Laborer : ILaborer
{
    #region Constants

    protected const string OperationTemplate = "({Operation:l}) {message:l}";

    #endregion

    #region Fields

    // todo clean
    //private long _state;
    //private readonly SemaphoreSlim _semaphore;

    //private ObjectTag? _tag;
    //private readonly object _tagLock;

    private readonly SemaphoreSlim _controlSemaphore;

    private readonly object _stateLock;
    private LaborerState _laborerState;
    private string? _name;
    private ObjectTag? _tagush; // todo rename

    #endregion

    #region ctor

    protected Laborer(ILogger? logger)
    {
        // todo clean

        this.OriginalLogger = logger;

        this.ContextLogger = logger?
            .ForContext(new ObjectTagEnricher(this.GetTag));

        _laborerState = LaborerState.Stopped;
        _controlSemaphore = new SemaphoreSlim(1, 1);
        _stateLock = new object();

        //_state = (long)LaborerState.Stopped;
        //_semaphore = new SemaphoreSlim(1, 1);
        //_tagLock = new object();
    }

    #endregion

    #region Abstract

    protected abstract void DisposeImpl();

    protected abstract bool IsPausingSupported { get; }

    #endregion

    #region State

    private void SetState(LaborerState state)
    {
        lock (_stateLock) // lock for 'SetState'
        {
            _laborerState = state;
            this.ClearTag();
        }

        // todo clean
        //Interlocked.Exchange(ref _state, (long)state);
        //this.ClearTag();
    }

    private LaborerState GetState()
    {
        lock (_stateLock) // lock for 'GetState'
        {
            return _laborerState;
        }

        // todo clean
        //var state = Interlocked.Read(ref _state);
        //return (LaborerState)state;
    }

    #endregion

    #region Tag

    protected virtual ObjectTag BuildTag()
    {
        string? type;
        string? name;
        string? state;

        lock (_stateLock) // lock for 'BuildTag'
        {
            type = this.GetType().Name;
            name = _name;
            state = _laborerState.ToString();
        }

        return new ObjectTag(type, name, state);
    }

    protected ObjectTag GetTag()
    {
        lock (_stateLock) // lock for 'GetTag'
        {
            _tagush ??= this.BuildTag();
            return _tagush.Value;
        }

        // todo clean
        //lock (_tagLock)
        //{
        //    _tag ??= this.BuildTag();

        //    return _tag.Value;
        //}
    }

    private void ClearTag()
    {
        // guarded with lock

        _tagush = null;

        // todo clean
        //lock (_tagLock)
        //{
        //    _tag = null;
        //}
    }

    #endregion

    #region Protected

    protected virtual ValueTask DisposeAsyncImpl()
    {
        this.DisposeImpl();
        return default;
    }

    protected bool IsDisposed => this.State == LaborerState.Disposed; // todo: move to extensions

    protected void WithStateLock(Action action)
    {
        lock (_stateLock) // lock for 'WithStateLock'
        {
            action();
        }
    }

    protected ILogger? OriginalLogger { get; }

    protected ILogger? ContextLogger { get; }

    protected string GetNameForDiagnostics() => this.Name ?? this.GetType().FullName!;

    protected void CheckNotDisposed()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetNameForDiagnostics());
        }
    }

    protected void AllowIfStateIs(string requestedOperation, LaborerState allowedStates)
    {
        this.CheckNotDisposed();

        throw new NotImplementedException();
    }

    #endregion

    #region State Change Handlers

    #region Start

    #region Sync

    protected virtual void OnBeforeStarting()
    {
        // idle
    }

    protected virtual void OnStarting()
    {
        // idle
    }

    protected virtual void OnAfterStarted()
    {
        // idle
    }

    #endregion

    #region Async

    protected virtual Task OnBeforeStartingAsync(CancellationToken cancellationToken)
    {
        this.OnBeforeStarting();
        return Task.CompletedTask;
    }

    protected virtual Task OnStartingAsync(CancellationToken cancellationToken)
    {
        this.OnStarting();
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterStartedAsync(CancellationToken cancellationToken)
    {
        this.OnAfterStarted();
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region Stop

    #region Sync

    protected virtual void OnBeforeStopping()
    {
        // idle
    }

    protected virtual void OnStopping()
    {
        // idle
    }

    protected virtual void OnAfterStopped()
    {
        // idle
    }

    #endregion

    #region Async

    protected virtual Task OnBeforeStoppingAsync(CancellationToken cancellationToken)
    {
        this.OnBeforeStopping();
        return Task.CompletedTask;
    }

    protected virtual Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        this.OnStopping();
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterStoppedAsync(CancellationToken cancellationToken)
    {
        this.OnAfterStopped();
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region Pause

    #region Sync

    protected virtual void OnBeforePausing()
    {
        // idle
    }

    protected virtual void OnPausing()
    {
        // idle
    }

    protected virtual void OnAfterPaused()
    {
        // idle
    }

    #endregion

    #region Async

    protected virtual Task OnBeforePausingAsync(CancellationToken cancellationToken)
    {
        this.OnBeforePausing();
        return Task.CompletedTask;
    }

    protected virtual Task OnPausingAsync(CancellationToken cancellationToken)
    {
        this.OnPausing();
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterPausedAsync(CancellationToken cancellationToken)
    {
        this.OnAfterPaused();
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region Resume

    #region Sync

    protected virtual void OnBeforeResuming()
    {
        // idle
    }

    protected virtual void OnResuming()
    {
        // idle
    }

    protected virtual void OnAfterResumed()
    {
        // idle
    }

    #endregion

    #region Async

    protected virtual Task OnBeforeResumingAsync(CancellationToken cancellationToken)
    {
        this.OnBeforeResuming();
        return Task.CompletedTask;
    }

    protected virtual Task OnResumingAsync(CancellationToken cancellationToken)
    {
        this.OnResuming();
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterResumedAsync(CancellationToken cancellationToken)
    {
        this.OnAfterResumed();
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #endregion

    #region ILaborer Members

    public string? Name
    {
        get
        {
            lock (_stateLock) // lock for get_Name
            {
                return _name;
            }
        }
        set
        {
            lock (_stateLock) // lock for set_Name
            {
                this.CheckNotDisposed();

                _name = value;
                this.ClearTag();
            }
        }
    }

    public LaborerState State => this.GetState();

    public void Start()
    {
        const string operation = nameof(Start);
        this.AllowIfStateIs(operation, LaborerState.Stopped);

        void VerboseLog(string message) =>
            this.ContextLogger?.Verbose(OperationTemplate, operation, message);

        void ErrorLog(Exception? ex, string? message) =>
            this.ContextLogger?.Error(ex, OperationTemplate, operation, message);

        VerboseLog("Entered.");

        _controlSemaphore.Wait();

        var initialState = this.State;

        try
        {
            VerboseLog($"About to call {nameof(OnBeforeStarting)}.");
            this.OnBeforeStarting();
            this.SetState(LaborerState.Starting);

            VerboseLog($"About to call {nameof(OnStarting)}.");
            this.OnStarting();
            this.SetState(LaborerState.Running);

            VerboseLog($"About to call {nameof(OnAfterStarted)}.");
            this.OnAfterStarted();
        }
        catch (Exception ex)
        {
            this.SetState(initialState);
            ErrorLog(ex, "Exception was thrown.");
            throw;
        }
        finally
        {
            _controlSemaphore.Release();
        }

        VerboseLog("Leaving.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string operation = nameof(Start);
        this.AllowIfStateIs(operation, LaborerState.Stopped);

        void VerboseLog(string message) =>
            this.ContextLogger?.Verbose(OperationTemplate, operation, message);

        void ErrorLog(Exception? ex, string? message) =>
            this.ContextLogger?.Error(ex, OperationTemplate, operation, message);

        VerboseLog("Entered.");

        await _controlSemaphore.WaitAsync(cancellationToken);

        var initialState = this.State;

        try
        {
            VerboseLog($"About to call {nameof(OnBeforeStartingAsync)}.");
            await this.OnBeforeStartingAsync(cancellationToken);
            this.SetState(LaborerState.Starting);

            VerboseLog($"About to call {nameof(OnStartingAsync)}.");
            await this.OnStartingAsync(cancellationToken);
            this.SetState(LaborerState.Running);

            VerboseLog($"About to call {nameof(OnAfterStartedAsync)}.");
            await this.OnAfterStartedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this.SetState(initialState);
            ErrorLog(ex, "Exception was thrown.");
            throw;
        }
        finally
        {
            _controlSemaphore.Release();
        }

        VerboseLog("Leaving.");
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public Task PauseAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Resume()
    {
        throw new NotImplementedException();
    }

    public Task ResumeAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        // todo: Severity	Code	Description	Project	File	Line	Suppression State
        //Message CA1816  Change Laborer.Dispose() to call GC.SuppressFinalize(object).This will prevent derived types that introduce a finalizer from needing to re - implement 'IDisposable' to call it.TauCode.Working D:\work\tau\lib2\working\src\TauCode.Working\Labor\Laborer.cs   305 Active

        const string operation = nameof(Dispose);

        void VerboseLog(string message) =>
            this.ContextLogger?.Verbose(OperationTemplate, operation, message);

        void ErrorLog(Exception? ex, string? message) =>
            this.ContextLogger?.Error(ex, OperationTemplate, operation, message);

        VerboseLog("Entered.");

        if (this.IsDisposed)
        {
            VerboseLog("Already disposed, exiting.");
            return;
        }

        _controlSemaphore.Wait();
        try
        {
            this.DisposeImpl();
            VerboseLog($"'{nameof(DisposeImpl)}' invoked.");
        }
        catch (Exception ex)
        {
            ErrorLog(ex, $"'{nameof(DisposeImpl)}' threw an exception.");
            throw;
        }
        finally
        {
            this.SetState(LaborerState.Disposed);
            _controlSemaphore.Release();
        }

        _controlSemaphore.Dispose();

        VerboseLog("Disposed.");
    }

    #endregion

    #region IAsyncDisposable Members

    public async ValueTask DisposeAsync()
    {
        // todo: Severity	Code	Description	Project	File	Line	Suppression State
        //Message CA1816  Change Laborer.Dispose() to call GC.SuppressFinalize(object).This will prevent derived types that introduce a finalizer from needing to re - implement 'IDisposable' to call it.TauCode.Working D:\work\tau\lib2\working\src\TauCode.Working\Labor\Laborer.cs   305 Active

        if (this.IsDisposed)
        {
            return;
        }

        await _controlSemaphore.WaitAsync();

        try
        {
            await this.DisposeAsyncImpl();
        }
        catch (Exception ex)
        {
            throw new NotImplementedException(ex.Message, ex); // todo: log exception
        }
        finally
        {
            this.SetState(LaborerState.Disposed);
            _controlSemaphore.Release();
        }
    }

    #endregion
}