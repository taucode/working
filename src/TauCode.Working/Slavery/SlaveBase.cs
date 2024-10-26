using Serilog;
using System.Text;
using TauCode.Infrastructure.Logging;

namespace TauCode.Working.Slavery;

public abstract class SlaveBase : ISlave
{
    #region Fields

    private long _stateValue;
    private long _isDisposedValue;

    private string? _name;

    private readonly object _controlLock;

    #endregion

    #region Constructor

    protected SlaveBase(ILogger? logger)
    {
        OriginalLogger = logger;

        ContextLogger = logger?
            .ForContext(new SlaveObjectTagEnricher(GetTag));

        _controlLock = new object();

        SetState(SlaveState.Stopped);
        SetIsDisposed(false);
    }

    #endregion

    #region Private

    private SlaveState GetState()
    {
        var stateValue = Interlocked.Read(ref _stateValue);
        return (SlaveState)stateValue;
    }

    private void SetState(SlaveState state)
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
        SlaveState[] allowedInitialStableStates,
        SlaveState targetTransientState,
        SlaveState targetStableState,
        Action beforeAction,
        string beforeActionName,
        Action innerAction,
        string innerActionName,
        Action afterAction,
        string afterActionName)
    {
        lock (_controlLock)
        {
            CheckNotDisposed();
            AllowIfStateIs(actionName, allowedInitialStableStates);

            var initialStableState = GetState();

            VerboseLogOperation(actionName, "Initializing operation. ");

            #region 'before'

            try
            {
                beforeAction();

                ContextLogger?.Verbose(
                    "'{0:l}'. '{1:l}' called successfully. State is '{2}'.",
                    actionName,
                    beforeActionName,
                    GetState());
            }
            catch (Exception ex)
            {
                ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' has thrown an exception. State is '{2}'.",
                    actionName,
                    beforeActionName,
                    GetState());

                throw;
            }

            #endregion

            #region 'inner'

            // change to transient state
            SetState(targetTransientState);

            try
            {
                innerAction();

                ContextLogger?.Verbose(
                    "'{0:l}'. '{1:l}' called successfully. State is '{2}'.",
                    actionName,
                    innerActionName,
                    GetState());
            }
            catch (Exception ex)
            {
                ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' has thrown an exception. State will be changed from current '{2}' to initial '{3}'.",
                    actionName,
                    innerActionName,
                    GetState(),
                    initialStableState);

                SetState(initialStableState);

                throw;
            }

            #endregion

            #region 'after'

            // change to target stable state
            SetState(targetStableState);

            try
            {
                afterAction();

                ContextLogger?.Verbose(
                    "'{0:l}'. '{1:l}' called successfully. State is '{2}'.",
                    actionName,
                    afterActionName,
                    GetState());
            }
            catch (Exception ex)
            {
                ContextLogger?.Verbose(
                    ex,
                    "'{0:l}'. '{1:l}' has thrown an exception. Current state is '{2}' and it will be kept.",
                    actionName,
                    afterActionName,
                    GetState());

                throw;
            }

            #endregion

            VerboseLogOperation(actionName, "Operation completed successfully. ");
        }
    }

    private NotSupportedException CreatePausingNotSupportedException()
    {
        return new NotSupportedException("Pausing/resuming is not supported.");
    }

    private void VerboseLogOperation(string operationName, string additionalInfo = "")
    {
        if (ContextLogger != null)
        {
            ContextLogger.Verbose(
                "'{0:l}'. {1:l}State is '{2}'.",
                operationName,
                additionalInfo,
                GetState());
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
        VerboseLogOperation(nameof(OnBeforeStarting));
    }

    protected virtual void OnStarting()
    {
        VerboseLogOperation(nameof(OnStarting));
    }

    protected virtual void OnAfterStarted()
    {
        VerboseLogOperation(nameof(OnAfterStarted));
    }

    #endregion

    #region Stopping

    protected virtual void OnBeforeStopping()
    {
        VerboseLogOperation(nameof(OnBeforeStopping));
    }

    protected virtual void OnStopping()
    {
        VerboseLogOperation(nameof(OnStopping));
    }

    protected virtual void OnAfterStopped()
    {
        VerboseLogOperation(nameof(OnAfterStopped));
    }

    #endregion

    #region Pausing

    protected virtual void OnBeforePausing()
    {
        VerboseLogOperation(nameof(OnBeforePausing));
    }

    protected virtual void OnPausing()
    {
        VerboseLogOperation(nameof(OnPausing));
    }

    protected virtual void OnAfterPaused()
    {
        VerboseLogOperation(nameof(OnAfterPaused));
    }

    #endregion

    #region Resuming

    protected virtual void OnBeforeResuming()
    {
        VerboseLogOperation(nameof(OnBeforeResuming));
    }

    protected virtual void OnResuming()
    {
        VerboseLogOperation(nameof(OnResuming));
    }

    protected virtual void OnAfterResumed()
    {
        VerboseLogOperation(nameof(OnAfterResumed));
    }

    #endregion

    #region Disposing

    protected virtual void OnAfterDisposed()
    {
        VerboseLogOperation(nameof(OnAfterDisposed));
    }

    #endregion

    #endregion

    protected virtual ObjectTag GetTag()
    {
        return new ObjectTag(GetType().Name, Name, null);
    }

    protected ILogger? OriginalLogger { get; }

    protected ILogger? ContextLogger { get; set; }

    protected void Stop(bool throwOnDisposedOrWrongState)
    {
        lock (_controlLock)
        {
            if (GetIsDisposed())
            {
                if (throwOnDisposedOrWrongState)
                {
                    CheckNotDisposed(); // will throw
                }
                else
                {
                    return;
                }
            }

            var state = GetState();
            var isValidState =
                state == SlaveState.Running ||
                state == SlaveState.Paused;

            if (!isValidState)
            {
                // actually, the only real option here is 'state == SlaveState.Stopped'
                // transient states (Starting, Stopping, Pausing, Resuming) will never be here because of '_controlLock'

                if (throwOnDisposedOrWrongState)
                {
                    throw CreateInvalidOperationException(nameof(Stop), state);
                }
                else
                {
                    return;
                }
            }

            ChangeStableState(
                $"{nameof(Stop)}(bool)",
                new[]
                {
                    SlaveState.Running,
                    SlaveState.Paused,
                },
                SlaveState.Stopping,
                SlaveState.Stopped,
                OnBeforeStopping,
                nameof(OnBeforeStopping),
                OnStopping,
                nameof(OnStopping),
                OnAfterStopped,
                nameof(OnAfterStopped));
        }
    }

    protected InvalidOperationException CreateInvalidOperationException(string requestedOperation, SlaveState actualState)
    {
        var sb = new StringBuilder();
        sb.Append($"Cannot perform operation '{requestedOperation}'. Slave state is '{actualState}'. Slave name is '{GetNameForDiagnostics()}'.");

        var message = sb.ToString();

        return new InvalidOperationException(message);
    }

    protected void ProhibitIfStateIs(string requestedOperation, params SlaveState[] prohibitedStates)
    {
        var actualState = GetState();
        if (prohibitedStates.Contains(actualState))
        {
            throw CreateInvalidOperationException(requestedOperation, actualState);
        }
    }

    protected void AllowIfStateIs(string requestedOperation, params SlaveState[] allowedStates)
    {
        var actualState = GetState();
        if (allowedStates.Contains(actualState))
        {
            // ok
        }
        else
        {
            throw CreateInvalidOperationException(requestedOperation, actualState);
        }
    }

    protected string GetNameForDiagnostics() => _name ?? GetType().FullName!;

    protected void CheckNotDisposed()
    {
        if (GetIsDisposed())
        {
            throw new ObjectDisposedException(GetNameForDiagnostics());
        }
    }

    #endregion

    #region ISlave Members

    public string? Name
    {
        get => _name;
        set
        {
            CheckNotDisposed();

            _name = value;
        }
    }

    public SlaveState State => GetState();

    public void Start()
    {
        ChangeStableState(
            nameof(Start),
            new[]
            {
                SlaveState.Stopped
            },
            SlaveState.Starting,
            SlaveState.Running,
            OnBeforeStarting,
            nameof(OnBeforeStarting),
            OnStarting,
            nameof(OnStarting),
            OnAfterStarted,
            nameof(OnAfterStarted));
    }

    public void Stop() => Stop(true);

    public void Pause()
    {
        if (!IsPausingSupported)
        {
            throw CreatePausingNotSupportedException();
        }

        ChangeStableState(
            nameof(Pause),
            new[]
            {
                SlaveState.Running,
            },
            SlaveState.Pausing,
            SlaveState.Paused,
            OnBeforePausing,
            nameof(OnBeforePausing),
            OnPausing,
            nameof(OnPausing),
            OnAfterPaused,
            nameof(OnAfterPaused));
    }

    public void Resume()
    {
        if (!IsPausingSupported)
        {
            throw CreatePausingNotSupportedException();
        }

        ChangeStableState(
            nameof(Resume),
            new[]
            {
                SlaveState.Paused,
            },
            SlaveState.Resuming,
            SlaveState.Running,
            OnBeforeResuming,
            nameof(OnBeforeResuming),
            OnResuming,
            nameof(OnResuming),
            OnAfterResumed,
            nameof(OnAfterResumed));
    }

    public bool IsDisposed => GetIsDisposed();

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        lock (_controlLock)
        {
            if (GetIsDisposed())
            {
                return; // won't dispose twice
            }

            VerboseLogOperation(nameof(Dispose));

            Stop(false); // can throw; in this case, 'Dispose' will be incomplete.

            SetIsDisposed(true);

            try
            {
                OnAfterDisposed();
                VerboseLogOperation(nameof(Dispose), "Disposed successfully. ");
            }
            catch (Exception ex)
            {
                ContextLogger?.Verbose(
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