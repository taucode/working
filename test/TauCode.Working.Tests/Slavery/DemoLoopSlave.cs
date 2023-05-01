using Serilog;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

public class DemoLoopSlave : LoopSlaveBase
{
    public DemoLoopSlave(ILogger? logger)
        : base(logger)
    {
    }

    public override bool IsPausingSupported => true;

    protected override async Task<TimeSpan> DoWork(CancellationToken cancellationToken)
    {
        if (WorkAction == null)
        {
            throw new InvalidOperationException($"Cannot run: '{nameof(WorkAction)}' is null.");
        }

        return await WorkAction(this, cancellationToken);
    }

    public void WriteInformationToLog(string text)
    {
        ContextLogger?.Information(text);
    }

    public Func<DemoLoopSlave, CancellationToken, Task<TimeSpan>>? WorkAction { get; set; }
}