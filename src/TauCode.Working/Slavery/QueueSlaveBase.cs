using Serilog;

namespace TauCode.Working.Slavery;

public abstract class QueueSlaveBase<TAssignment> : LoopSlaveBase
{
    private readonly Queue<TAssignment> _assignments;
    private readonly object _lock;

    protected QueueSlaveBase(ILogger? logger)
        : base(logger)
    {
        _assignments = new Queue<TAssignment>();
        _lock = new object();
    }

    public void AddAssignment(TAssignment assignment)
    {
        CheckNotDisposed();
        ProhibitIfStateIs(nameof(AddAssignment), SlaveState.Stopped, SlaveState.Stopping);

        CheckAssignment(assignment);

        lock (_lock)
        {
            _assignments.Enqueue(assignment);
        }

        AbortVacation();
    }

    protected abstract Task DoAssignment(TAssignment assignment, CancellationToken cancellationToken);

    protected virtual void CheckAssignment(TAssignment assignment)
    {
        // idle.
    }

    protected override void OnAfterStopped()
    {
        base.OnAfterStopped();

        lock (_lock)
        {
            _assignments.Clear();
        }
    }

    public override bool IsPausingSupported => true;

    protected override async Task<TimeSpan> DoWork(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TAssignment assignment;

            lock (_lock)
            {
                if (_assignments.Count > 0)
                {
                    assignment = _assignments.Dequeue();
                }
                else
                {
                    return VeryLongVacation; // no assignments to work on.
                }
            }

            try
            {
                await DoAssignment(assignment, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw; // task was canceled for reason
            }
            catch (Exception ex)
            {
                ContextLogger?.Error(ex, "Exception thrown while processing the assignment.");
            }
        }
    }
}