using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Name_Disposed_CanBeGot()
    {
        // Arrange
        var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Dispose();

        // Act
        var gotName = worker.Name;
        var ex = Assert.Throws<ObjectDisposedException>(() => worker.Name = null)!;

        // Assert
        Assert.That(gotName, Is.EqualTo("Psi"));
        Assert.That(ex, Has.Message.StartWith("Cannot access a disposed object."));
        Assert.That(ex.ObjectName, Is.EqualTo("Psi"));
    }
}