using Serilog;

namespace TauCode.Working;

public abstract class QueueWorkerBase<TAssignment> : LoopWorkerBase
{
    private readonly Queue<TAssignment> _assignments;
    private readonly object _lock;

    protected QueueWorkerBase(ILogger? logger)
        : base(logger)
    {
        _assignments = new Queue<TAssignment>();
        _lock = new object();
    }

    public void AddAssignment(TAssignment assignment)
    {
        this.CheckNotDisposed();
        this.ProhibitIfStateIs(nameof(AddAssignment), WorkerState.Stopped, WorkerState.Stopping);

        // todo clean
        //var state = this.State;
        //if (state == WorkerState.Stopped || state == WorkerState.Stopping)
        //{
        //    throw this.CreateInvalidOperationException(nameof(AddAssignment), state);
        //}

        this.CheckAssignment(assignment);

        lock (_lock)
        {
            _assignments.Enqueue(assignment);
        }

        this.AbortVacation();
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
                await this.DoAssignment(assignment, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw; // task was canceled for reason
            }
            catch (Exception ex)
            {
                this.ContextLogger?.Error(ex, "Exception thrown while processing the assignment.");
            }
        }
    }
}