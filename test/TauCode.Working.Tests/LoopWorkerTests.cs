using NUnit.Framework;
using Serilog;
using System.Text;
using TauCode.IO;

namespace TauCode.Working.Tests;

[TestFixture]
public class LoopWorkerTests
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
        using var worker = new DemoLoopWorker(_logger)
        {
            Name = "Psi",
        };

        worker.WorkAction = async (@base, token) =>
        {
            @base.WriteInformationToLog("hello");
            await Task.Delay(100, token);
            return TimeSpan.FromMilliseconds(200);
        };

        // Act
        worker.Start();
        await Task.Delay(400);
        worker.Stop();
        worker.Dispose();

        // Assert
        var log = _writer.ToString();
        Assert.That(log, Does.Contain("hello"));
    }

    [Test]
    public async Task Resume_ValidInput_DoesWork()
    {
        // Arrange
        using var worker = new DemoLoopWorker(_logger)
        {
            Name = "Psi",
        };

        worker.WorkAction = async (@base, token) =>
        {
            @base.WriteInformationToLog("hello");
            await Task.Delay(200, token);
            return TimeSpan.FromMilliseconds(300);
        };

        // Act
        worker.Start();

        await Task.Delay(100);
        worker.Pause();

        await Task.Delay(100);
        worker.Resume();

        await Task.Delay(250);
        worker.Dispose();

        // Assert
        var log = _writer.ToString();
        Assert.That(log, Does.Contain("hello"));
    }
}