using Serilog;
using TauCode.Working.Labor;

namespace TauCode.Working.Tests.Labor;

public class TestLaborer : Laborer
{
    public TestLaborer(ILogger? logger)
        : base(logger)
    {

    }

    public Action? CustomDispose { get; set; }

    protected override void DisposeImpl()
    {
        this.CustomDispose?.Invoke();
    }

    protected override bool IsPausingSupported => true;

    protected override ValueTask DisposeAsyncImpl() => default;
}