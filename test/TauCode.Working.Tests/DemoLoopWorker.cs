using Serilog;

namespace TauCode.Working.Tests;

public class DemoLoopWorker : LoopWorkerBase
{
    public DemoLoopWorker(ILogger? logger)
        : base(logger)
    {
    }

    public override bool IsPausingSupported => true;

    protected override async Task<TimeSpan> DoWork(CancellationToken cancellationToken)
    {
        if (this.WorkAction == null)
        {
            throw new InvalidOperationException($"Cannot run: '{nameof(WorkAction)}' is null.");
        }

        return await this.WorkAction(this, cancellationToken);
    }

    public void WriteInformationToLog(string text)
    {
        this.ContextLogger?.Information(text);
    }

    public Func<DemoLoopWorker, CancellationToken, Task<TimeSpan>>? WorkAction { get; set; }
}