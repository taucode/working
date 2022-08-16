using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Constructor_NoArguments_RunsOk()
    {
        // Arrange

        // Act
        using IWorker worker = new DemoWorker(_logger);

        // Assert
        Assert.That(worker.Name, Is.Null);
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);
    }
}