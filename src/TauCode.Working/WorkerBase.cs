using Serilog;
using System.Text;
using TauCode.Infrastructure.Logging;

namespace TauCode.Working;

public abstract class WorkerBase : IWorker
{
    #region Fields

    private long _stateValue;
    private long _isDisposedValue;

    private string? _name;

    private readonly object _controlLock;

    #endregion

    #region Constructor

    protected WorkerBase(ILogger? logger)
    {
        this.OriginalLogger = logger;

        this.ContextLogger = logger?
            .ForContext(new ObjectTagEnricher(this.GetTag));

        _controlLock = new object();

        this.SetState(WorkerState.Stopped);
        this.SetIsDisposed(false);
    }

    #endregion

    #region Private

    private WorkerState GetState()
    {
        var stateValue = Interlocked.Read(ref _stateValue);
        return (WorkerState)stateValue;
    }

    private void SetState(WorkerState state)
    {
        var stateValue = (long)state;
        Interlocked.Exchange(ref _stateValue, stateValue);
    }

    private bool GetIsDisposed()
    {
        var isDisposedValue = Interlocked.Read(ref _isDisposedValue);
        return isDisposedValue == 1L;
    }

    private void SetIsDisposed(bool isDisposed)
    {
        var isDisposedValue = isDisposed ? 1L : 0L;
        Interlocked.Exchange(ref _isDisposedValue, isDisposedValue);
    }

    private void ChangeStableState(
        string actionName,
        WorkerState[] allowedInitialStableStates,
        WorkerState targetTransientState,
        WorkerState targetStableState,
        Action beforeAction,
        string beforeActionName,
        Action innerAction,
        string innerActionName,
        Action afterAction,
        string afterActionName)
    {
        lock (_controlLock)
        {
            this.CheckNotDisposed();
            this.AllowIfStateIs(actionName, allowedInitialStableStates);

            var initialStableState = this.GetState();

            this.VerboseLogOperation(actionName, "Initializing operation. ");

            #region 'before'

            try
            {
                beforeAction();

                this.ContextLogger?.Verbose(
                    "'{0:l}'. '{1:l}' called successfully. State is '{2}'.",
                    actionName,
                    beforeActionName,
                    this.GetState());
            }
            catch (Exception ex)
            {
                this.ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' has thrown an exception. State is '{2}'.",
                    actionName,
                    beforeActionName,
                    this.GetState());

                throw;
            }

            #endregion

            #region 'inner'

            // change to transient state
            this.SetState(targetTransientState);

            try
            {
                innerAction();

                this.ContextLogger?.Verbose(
                    "'{0:l}'. '{1:l}' called successfully. State is '{2}'.",
                    actionName,
                    innerActionName,
                    this.GetState());
            }
            catch (Exception ex)
            {
                this.ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' has thrown an exception. State will be changed from current '{2}' to initial '{3}'.",
                    actionName,
                    innerActionName,
                    this.GetState(),
                    initialStableState);

                this.SetState(initialStableState);

                throw;
            }

            #endregion

            #region 'after'

            // change to target stable state
            this.SetState(targetStableState);

            try
            {
                afterAction();

                this.ContextLogger?.Verbose(
                    "'{0:l}'. '{1:l}' called successfully. State is '{2}'.",
                    actionName,
                    afterActionName,
                    this.GetState());
            }
            catch (Exception ex)
            {
                this.ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' has thrown an exception. Current state is '{2}' and it will be kept.",
                    actionName,
                    afterActionName,
                    this.GetState());

                throw;
            }

            #endregion

            this.VerboseLogOperation(actionName, "Operation completed successfully. ");
        }
    }

    private NotSupportedException CreatePausingNotSupportedException()
    {
        return new NotSupportedException("Pausing/resuming is not supported.");
    }

    private void VerboseLogOperation(string operationName, string additionalInfo = "")
    {
        if (this.ContextLogger != null)
        {
            this.ContextLogger.Verbose(
                "'{0:l}'. {1:l}State is '{2}'.",
                operationName,
                additionalInfo,
                this.GetState());
        }
    }

    #endregion

    #region Abstract

    public abstract bool IsPausingSupported { get; }

    #endregion

    #region Protected

    #region Handling state changing

    #region Starting

    protected virtual void OnBeforeStarting()
    {
        this.VerboseLogOperation(nameof(OnBeforeStarting));
    }

    protected virtual void OnStarting()
    {
        this.VerboseLogOperation(nameof(OnStarting));
    }

    protected virtual void OnAfterStarted()
    {
        this.VerboseLogOperation(nameof(OnAfterStarted));
    }

    #endregion

    #region Stopping

    protected virtual void OnBeforeStopping()
    {
        this.VerboseLogOperation(nameof(OnBeforeStopping));
    }

    protected virtual void OnStopping()
    {
        this.VerboseLogOperation(nameof(OnStopping));
    }

    protected virtual void OnAfterStopped()
    {
        this.VerboseLogOperation(nameof(OnAfterStopped));
    }

    #endregion

    #region Pausing

    protected virtual void OnBeforePausing()
    {
        this.VerboseLogOperation(nameof(OnBeforePausing));
    }

    protected virtual void OnPausing()
    {
        this.VerboseLogOperation(nameof(OnPausing));
    }

    protected virtual void OnAfterPaused()
    {
        this.VerboseLogOperation(nameof(OnAfterPaused));
    }

    #endregion

    #region Resuming

    protected virtual void OnBeforeResuming()
    {
        this.VerboseLogOperation(nameof(OnBeforeResuming));
    }

    protected virtual void OnResuming()
    {
        this.VerboseLogOperation(nameof(OnResuming));
    }

    protected virtual void OnAfterResumed()
    {
        this.VerboseLogOperation(nameof(OnAfterResumed));
    }

    #endregion

    #region Disposing

    protected virtual void OnAfterDisposed()
    {
        this.VerboseLogOperation(nameof(OnAfterDisposed));
    }

    #endregion

    #endregion

    protected virtual ObjectTag GetTag()
    {
        return new ObjectTag(this.GetType().Name!, this.Name);
    }

    protected ILogger? OriginalLogger { get; }

    protected ILogger? ContextLogger { get; set; }

    protected void Stop(bool throwOnDisposedOrWrongState)
    {
        lock (_controlLock)
        {
            if (this.GetIsDisposed())
            {
                if (throwOnDisposedOrWrongState)
                {
                    this.CheckNotDisposed(); // will throw
                }
                else
                {
                    return;
                }
            }

            var state = this.GetState();
            var isValidState =
                state == WorkerState.Running ||
                state == WorkerState.Paused;

            if (!isValidState)
            {
                // actually, the only real option here is 'state == WorkerState.Stopped'
                // transient states (Starting, Stopping, Pausing, Resuming) will never be here because of '_controlLock'

                if (throwOnDisposedOrWrongState)
                {
                    throw this.CreateInvalidOperationException(nameof(Stop), state);
                }
                else
                {
                    return;
                }
            }

            this.ChangeStableState(
                $"{nameof(Stop)}(bool)",
                new[]
                {
                    WorkerState.Running,
                    WorkerState.Paused,
                },
                WorkerState.Stopping,
                WorkerState.Stopped,
                this.OnBeforeStopping,
                nameof(OnBeforeStopping),
                this.OnStopping,
                nameof(OnStopping),
                this.OnAfterStopped,
                nameof(OnAfterStopped));
        }
    }

    protected InvalidOperationException CreateInvalidOperationException(string requestedOperation, WorkerState actualState)
    {
        var sb = new StringBuilder();
        sb.Append($"Cannot perform operation '{requestedOperation}'. Worker state is '{actualState}'. Worker name is '{this.GetNameForDiagnostics()}'.");

        var message = sb.ToString();

        return new InvalidOperationException(message);
    }

    protected void ProhibitIfStateIs(string requestedOperation, params WorkerState[] prohibitedStates)
    {
        var actualState = this.GetState();
        if (prohibitedStates.Contains(actualState))
        {
            throw this.CreateInvalidOperationException(requestedOperation, actualState);
        }
    }

    protected void AllowIfStateIs(string requestedOperation, params WorkerState[] allowedStates)
    {
        var actualState = this.GetState();
        if (allowedStates.Contains(actualState))
        {
            // ok
        }
        else
        {
            throw this.CreateInvalidOperationException(requestedOperation, actualState);
        }
    }

    protected string GetNameForDiagnostics() => _name ?? this.GetType().FullName!;

    protected void CheckNotDisposed()
    {
        if (this.GetIsDisposed())
        {
            throw new ObjectDisposedException(this.GetNameForDiagnostics());
        }
    }

    #endregion

    #region IWorker Members

    public string? Name
    {
        get => _name;
        set
        {
            this.CheckNotDisposed();

            _name = value;
        }
    }

    public WorkerState State => this.GetState();

    public void Start()
    {
        this.ChangeStableState(
            nameof(Start),
            new[]
            {
                WorkerState.Stopped
            },
            WorkerState.Starting,
            WorkerState.Running,
            this.OnBeforeStarting,
            nameof(OnBeforeStarting),
            this.OnStarting,
            nameof(OnStarting),
            this.OnAfterStarted,
            nameof(OnAfterStarted));
    }

    public void Stop() => this.Stop(true);

    public void Pause()
    {
        if (!IsPausingSupported)
        {
            throw this.CreatePausingNotSupportedException();
        }

        this.ChangeStableState(
            nameof(Pause),
            new[]
            {
                WorkerState.Running,
            },
            WorkerState.Pausing,
            WorkerState.Paused,
            this.OnBeforePausing,
            nameof(OnBeforePausing),
            this.OnPausing,
            nameof(OnPausing),
            this.OnAfterPaused,
            nameof(OnAfterPaused));
    }

    public void Resume()
    {
        if (!IsPausingSupported)
        {
            throw this.CreatePausingNotSupportedException();
        }

        this.ChangeStableState(
            nameof(Resume),
            new[]
            {
                WorkerState.Paused,
            },
            WorkerState.Resuming,
            WorkerState.Running,
            this.OnBeforeResuming,
            nameof(OnBeforeResuming),
            this.OnResuming,
            nameof(OnResuming),
            this.OnAfterResumed,
            nameof(OnAfterResumed));
    }

    public bool IsDisposed => this.GetIsDisposed();

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        lock (_controlLock)
        {
            if (this.GetIsDisposed())
            {
                return; // won't dispose twice
            }

            this.VerboseLogOperation(nameof(Dispose));

            this.Stop(false); // can throw; in this case, 'Dispose' will be incomplete.

            this.SetIsDisposed(true);

            try
            {
                this.OnAfterDisposed();
                this.VerboseLogOperation(nameof(Dispose), "Disposed successfully. ");
            }
            catch (Exception ex)
            {
                this.ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' thrown an exception. Object is disposed anyways.",
                    nameof(Dispose),
                    nameof(OnAfterDisposed));

                throw;
            }
        }
    }

    #endregion
}