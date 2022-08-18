using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Dispose_Stopped_Disposes()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,
            }));
    }

    [Test]
    public async Task Dispose_Starting_WaitsThenDisposes()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnStoppingTimeout = TimeSpan.FromSeconds(1),
        };

        worker.Start();

        var stopTask = new Task(() => worker.Stop());
        stopTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public void Dispose_Running_Disposes()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        worker.Start();

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public async Task Dispose_Stopping_WaitsThenDisposes()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnStoppingTimeout = TimeSpan.FromSeconds(1),
        };

        worker.Start();

        var stopTask = new Task(() => worker.Stop());
        stopTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public async Task Dispose_Pausing_WaitsThenDisposes()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnPausingTimeout = TimeSpan.FromSeconds(1),
        };

        worker.Start();

        var pauseTask = new Task(() => worker.Pause());
        pauseTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Pausing));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Pausing,
                WorkerState.Paused,

                WorkerState.Paused,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public void Dispose_Paused_Disposes()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        worker.Start();
        worker.Pause();

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Pausing,
                WorkerState.Paused,

                WorkerState.Paused,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public async Task Dispose_Resuming_WaitsThenDisposes()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnResumingTimeout = TimeSpan.FromSeconds(1),
        };

        worker.Start();
        worker.Pause();

        var pauseTask = new Task(() => worker.Resume());
        pauseTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Pausing,
                WorkerState.Paused,

                WorkerState.Paused,
                WorkerState.Resuming,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public void Dispose_Disposed_DoesNothing()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        worker.Dispose();

        var stateBeforeAction = worker.State;

        // Act
        worker.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,
            }));
    }

    [Test]
    public void Dispose_ThrowsOnDisposing_ThrowsAndDisposes()
    {
        // Arrange
        using var worker = new DemoWorker(_logger);

        worker.ThrowsOnAfterDisposed = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Dispose())!;

        // Assert
        Assert.That(ex.Message, Is.EqualTo("OnAfterDisposed failed!"));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.True);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,
            }));

        worker.ThrowsOnAfterDisposed = false; // let worker get disposed in peace.
    }
}