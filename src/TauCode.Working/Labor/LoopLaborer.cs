using Serilog;

namespace TauCode.Working.Labor;

public abstract class LoopLaborer : Laborer
{
    protected LoopLaborer(ILogger? logger)
        : base(logger)
    {
    }

    protected abstract Task<TimeSpan> DoWork(CancellationToken cancellationToken);
}