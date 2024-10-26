using NUnit.Framework;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Name_Disposed_CanBeGot()
    {
        // Arrange
        var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Dispose();

        // Act
        var gotName = slave.Name;
        var ex = Assert.Throws<ObjectDisposedException>(() => slave.Name = null)!;

        // Assert
        Assert.That(gotName, Is.EqualTo("Psi"));
        Assert.That(ex, Has.Message.StartWith("Cannot access a disposed object."));
        Assert.That(ex.ObjectName, Is.EqualTo("Psi"));
    }
}