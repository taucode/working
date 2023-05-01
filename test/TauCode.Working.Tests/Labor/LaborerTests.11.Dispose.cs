using NUnit.Framework;

namespace TauCode.Working.Tests.Labor;

[TestFixture]
public partial class LaborerTests
{
    [Test]
    public void Dispose_LaborerJustCreated_Disposes()
    {
        // Arrange
        using var laborer = new TestLaborer(_logger);

        // Act
        laborer.Dispose();

        // Assert
        var log = this.GetLogLines();
        var expectedLog = this.GetExpectedLog(nameof(Dispose_LaborerJustCreated_Disposes));
        Assert.That(log, Is.EqualTo(expectedLog));
    }

    [Test]
    public void Dispose_AlreadyDisposed_DoesNothing()
    {
        // Arrange
        using var laborer = new TestLaborer(_logger);
        laborer.Dispose();

        // Act
        laborer.Dispose();

        // Assert
        var logText = this.GetLog();
        var log = this.GetLogLines();
        var expectedLog = this.GetExpectedLog(nameof(Dispose_AlreadyDisposed_DoesNothing));
        Assert.That(log, Is.EqualTo(expectedLog));
    }
}
