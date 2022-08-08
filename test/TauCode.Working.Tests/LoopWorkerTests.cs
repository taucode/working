using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TauCode.Infrastructure.Logging;

namespace TauCode.Working.Tests;

[TestFixture]
public class LoopWorkerTests
{
    private StringLogger _logger;

    private string CurrentLog => _logger.ToString();

    [SetUp]
    public async Task SetUp()
    {
        _logger = new StringLogger();
        await Task.Delay(5); // let TPL initiate
    }

    // todo: make a full-scale test with good name
    [Test]
    public async Task TodoFoo()
    {
        // Arrange
        using var worker = new DemoLoopWorker
        {
            Name = "Psi",
        };

        worker.Logger = _logger;
        worker.WorkAction = async (@base, token) =>
        {
            @base.Logger.LogInformation("hello");
            await Task.Delay(100, token);
            return TimeSpan.FromMilliseconds(200);
        };

        // Act
        worker.Start();
        await Task.Delay(400);
        worker.Stop();

        // Assert
        worker.Dispose();
    }

    // todo: make a full-scale test with good name
    [Test]
    public async Task TodoFoo2()
    {
        // Arrange
        using var worker = new DemoLoopWorker
        {
            Name = "Psi",
        };

        worker.Logger = _logger;
        worker.WorkAction = async (@base, token) =>
        {
            @base.Logger.LogInformation("hello");
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

        // Assert
        worker.Dispose();
    }

}