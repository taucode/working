using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Stop_Stopped_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Stop());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,
            }));
    }

    [Test]
    public async Task Stop_Starting_WaitsThenStops()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnStartingTimeout = TimeSpan.FromSeconds(1),
        };

        var startTask = new Task(() => worker.Start());
        startTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = worker.State;

        // Act
        worker.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Starting));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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
                WorkerState.Stopped
            }));
    }

    [Test]
    public void Stop_Running_Stops()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        worker.Start();

        var stateBeforeAction = worker.State;

        // Act
        worker.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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
    public async Task Stop_Stopping_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Stop());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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
    public async Task Stop_Pausing_WaitsThenStops()
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
        worker.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Pausing));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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
    public void Stop_Paused_Stops()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnBeforeStartingTimeout = TimeSpan.FromSeconds(1),
        };

        worker.Start();
        worker.Pause();

        var stateBeforeAction = worker.State;

        // Act
        worker.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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
    public async Task Stop_Resuming_WaitsThenStops()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
            OnResumingTimeout = TimeSpan.FromSeconds(1),
        };

        worker.Start();
        worker.Pause();

        var resumeTask = new Task(() => worker.Resume());
        resumeTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = worker.State;

        // Act
        worker.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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
    public void Stop_WasStartedStoppedStarted_Stops()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();
        worker.Stop();
        worker.Start();

        var stateBeforeAction = worker.State;

        // Act
        worker.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.IsDisposed, Is.False);

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

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
                WorkerState.Stopping,
                WorkerState.Stopped,
            }));
    }

    [Test]
    public void Stop_Disposed_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();
        worker.Dispose();

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<ObjectDisposedException>(() => worker.Stop());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

        Assert.That(ex, Has.Message.StartWith("Cannot access a disposed object."));
        Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

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
    public void Stop_PausedThrowsOnBeforeStopping_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnBeforeStopping = true;

        worker.Start();
        worker.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeStopping failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Stop(bool)'. 'OnBeforeStopping' has thrown an exception. State is 'Paused'."));

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
            }));

        worker.ThrowsOnBeforeStopping = false; // let worker get disposed in peace.
    }

    [Test]
    public void Stop_PausedThrowsOnStopping_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnStopping = true;

        worker.Start();
        worker.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnStopping failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Stop(bool)'. 'OnStopping' has thrown an exception. State will be changed from current 'Stopping' to initial 'Paused'."));

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
            }));

        worker.ThrowsOnStopping = false; // let worker get disposed in peace.
    }

    [Test]
    public void Stop_PausedThrowsOnAfterStopped_ThrowsAndSetInStoppedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnAfterStopped = true;

        worker.Start();
        worker.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterStopped failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Stop(bool)'. 'OnAfterStopped' has thrown an exception. Current state is 'Stopped' and it will be kept."));

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

        worker.ThrowsOnAfterStopped = false; // let worker get disposed in peace.
    }

    [Test]
    public void Stop_RunningThrowsOnBeforeStopping_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnBeforeStopping = true;

        worker.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeStopping failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Stop(bool)'. 'OnBeforeStopping' has thrown an exception. State is 'Running'."));

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Running,
            }));

        worker.ThrowsOnBeforeStopping = false; // let worker get disposed in peace.
    }

    [Test]
    public void Stop_RunningThrowsOnStopping_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnStopping = true;

        worker.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnStopping failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Stop(bool)'. 'OnStopping' has thrown an exception. State will be changed from current 'Stopping' to initial 'Running'."));

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
            }));

        worker.ThrowsOnStopping = false; // let worker get disposed in peace.
    }

    [Test]
    public void Stop_RunningThrowsOnAfterStopped_ThrowsAndSetInStoppedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnAfterStopped = true;

        worker.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterStopped failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Stop(bool)'. 'OnAfterStopped' has thrown an exception. Current state is 'Stopped' and it will be kept."));

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

        worker.ThrowsOnAfterStopped = false; // let worker get disposed in peace.
    }
}