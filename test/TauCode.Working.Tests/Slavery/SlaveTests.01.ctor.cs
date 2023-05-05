using NUnit.Framework;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Constructor_NoArguments_RunsOk()
    {
        // Arrange

        // Act
        using ISlave slave = new DemoSlave(_logger);

        // Assert
        Assert.That(slave.Name, Is.Null);
        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));
        Assert.That(slave.IsDisposed, Is.False);
    }
}