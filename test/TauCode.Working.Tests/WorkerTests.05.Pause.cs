using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Pause_Stopped_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Stopped'. Worker name is 'Psi'."));

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
    public async Task Pause_Starting_WaitsThenPauses()
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
        worker.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Starting));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
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
            }));
    }

    [Test]
    public void Pause_Running_Pauses()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        worker.Start();

        var stateBeforeAction = worker.State;

        // Act
        worker.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
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
            }));
    }

    [Test]
    public async Task Pause_Stopping_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Stopped'. Worker name is 'Psi'."));

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
    public async Task Pause_Pausing_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Stopped'. Worker name is 'Psi'."));

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
    public void Pause_Paused_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();
        worker.Pause();

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Paused'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
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
            }));
    }

    [Test]
    public async Task Pause_Resuming_WaitsThenPauses()
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
        worker.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
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
                WorkerState.Pausing,
                WorkerState.Paused,
            }));
    }

    [Test]
    public void Pause_WasStartedPausedResumed_Pauses()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();
        worker.Pause();
        worker.Resume();

        var stateBeforeAction = worker.State;

        // Act
        worker.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
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
                WorkerState.Pausing,
                WorkerState.Paused,
            }));
    }

    [Test]
    public void Pause_Disposed_ThrowsException()
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
        var ex = Assert.Throws<ObjectDisposedException>(() => worker.Pause());

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
    public void Pause_ThrowsOnBeforePausing_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnBeforePausing = true;

        worker.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Pause())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforePausing failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Pause'. 'OnBeforePausing' has thrown an exception. State is 'Running'."));

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

    }

    [Test]
    public void Pause_ThrowsOnPausing_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnPausing = true;

        worker.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Pause())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnPausing failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Pause'. 'OnPausing' has thrown an exception. State will be changed from current 'Pausing' to initial 'Running'."));

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
            }));
    }

    [Test]
    public void Pause_ThrowsOnAfterPaused_ThrowsAndSetInPausedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnAfterPaused = true;

        worker.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Pause())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterPaused failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Pause'. 'OnAfterPaused' has thrown an exception. Current state is 'Paused' and it will be kept."));

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
            }));

    }
}