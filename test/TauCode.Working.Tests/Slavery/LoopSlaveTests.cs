using NUnit.Framework;
using Serilog;
using System.Text;
using TauCode.IO;

#pragma warning disable NUnit1032

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public class LoopSlaveTests
{
    private ILogger _logger = null!;
    private StringWriterWithEncoding _writer = null!;

    [SetUp]
    public async Task SetUp()
    {
        _writer = new StringWriterWithEncoding(Encoding.UTF8);
        _logger = new LoggerConfiguration()
            .WriteTo.TextWriter(_writer)
            .CreateLogger();

        await Task.Delay(5); // let TPL initiate
    }

    [Test]
    public async Task Start_ValidInput_DoesWork()
    {
        // Arrange
        using var slave = new DemoLoopSlave(_logger)
        {
            Name = "Psi",
        };

        slave.WorkAction = async (@base, token) =>
        {
            @base.WriteInformationToLog("hello");
            await Task.Delay(100, token);
            return TimeSpan.FromMilliseconds(200);
        };

        // Act
        slave.Start();
        await Task.Delay(400);
        slave.Stop();
        slave.Dispose();

        // Assert
        var log = _writer.ToString();
        Assert.That(log, Does.Contain("hello"));
    }

    [Test]
    public async Task Resume_ValidInput_DoesWork()
    {
        // Arrange
        using var slave = new DemoLoopSlave(_logger)
        {
            Name = "Psi",
        };

        slave.WorkAction = async (@base, token) =>
        {
            @base.WriteInformationToLog("hello");
            await Task.Delay(200, token);
            return TimeSpan.FromMilliseconds(300);
        };

        // Act
        slave.Start();

        await Task.Delay(100);
        slave.Pause();

        await Task.Delay(100);
        slave.Resume();

        await Task.Delay(250);
        slave.Dispose();

        // Assert
        var log = _writer.ToString();
        Assert.That(log, Does.Contain("hello"));
    }
}