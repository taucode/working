using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Resume_Stopped_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Stopped'. Worker name is 'Psi'."));

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
    public async Task Resume_Starting_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Starting));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Running'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.IsDisposed, Is.False);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running
            }));
    }

    [Test]
    public void Resume_Running_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Running'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.IsDisposed, Is.False);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running
            }));
    }

    [Test]
    public async Task Resume_Stopping_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Stopped'. Worker name is 'Psi'."));

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
    public async Task Resume_Pausing_WaitsThenResumes()
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
        worker.Resume();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Pausing));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
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
            }));
    }

    [Test]
    public void Resume_Paused_Resumes()
    {
        // Arrange
        using var worker = new DemoWorker(logger: _logger);

        worker.Start();
        worker.Pause();

        var stateBeforeAction = worker.State;

        // Act
        worker.Resume();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
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
            }));
    }

    [Test]
    public async Task Resume_Resuming_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Running'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
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
            }));
    }

    [Test]
    public void Resume_WasStartedPausedResumedPaused_Resumes()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();
        worker.Pause();
        worker.Resume();
        worker.Pause();

        var stateBeforeAction = worker.State;

        // Act
        worker.Resume();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
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

                WorkerState.Paused,
                WorkerState.Resuming,
                WorkerState.Running,
            }));
    }

    [Test]
    public void Resume_Disposed_ThrowsException()
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
        var ex = Assert.Throws<ObjectDisposedException>(() => worker.Resume());

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
    public void Resume_ThrowsOnBeforeResuming_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnBeforeResuming = true;

        worker.Start();
        worker.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Resume())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeResuming failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Resume'. 'OnBeforeResuming' has thrown an exception. State is 'Paused'."));

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

    }

    [Test]
    public void Resume_ThrowsOnResuming_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnResuming = true;

        worker.Start();
        worker.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Resume())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnResuming failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Resume'. 'OnResuming' has thrown an exception. State will be changed from current 'Resuming' to initial 'Paused'."));

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
            }));
    }

    [Test]
    public void Resume_ThrowsOnAfterResumed_ThrowsAndSetInRunningState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnAfterResumed = true;

        worker.Start();
        worker.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Resume())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterResumed failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Resume'. 'OnAfterResumed' has thrown an exception. Current state is 'Running' and it will be kept."));

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
            }));

    }
}