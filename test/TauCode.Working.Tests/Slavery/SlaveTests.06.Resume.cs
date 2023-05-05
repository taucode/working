using NUnit.Framework;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Resume_Stopped_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Slave state is 'Stopped'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,
            }));
    }

    [Test]
    public async Task Resume_Starting_WaitsThenThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
            OnStartingTimeout = TimeSpan.FromSeconds(1),
        };

        var startTask = new Task(() => slave.Start());
        startTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Starting));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Slave state is 'Running'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running
            }));
    }

    [Test]
    public void Resume_Running_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Slave state is 'Running'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running
            }));
    }

    [Test]
    public async Task Resume_Stopping_WaitsThenThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
            OnStoppingTimeout = TimeSpan.FromSeconds(1),
        };

        slave.Start();

        var stopTask = new Task(() => slave.Stop());
        stopTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Slave state is 'Stopped'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public async Task Resume_Pausing_WaitsThenResumes()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
            OnPausingTimeout = TimeSpan.FromSeconds(1),
        };

        slave.Start();

        var pauseTask = new Task(() => slave.Pause());
        pauseTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = slave.State;

        // Act
        slave.Resume();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Pausing));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,
            }));
    }

    [Test]
    public void Resume_Paused_Resumes()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        slave.Start();
        slave.Pause();

        var stateBeforeAction = slave.State;

        // Act
        slave.Resume();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Paused));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,
            }));
    }

    [Test]
    public async Task Resume_Resuming_WaitsThenThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
            OnResumingTimeout = TimeSpan.FromSeconds(1),
        };

        slave.Start();
        slave.Pause();

        var resumeTask = new Task(() => slave.Resume());
        resumeTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Resuming));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Slave state is 'Running'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,
            }));
    }

    [Test]
    public void Resume_WasStartedPausedResumedPaused_Resumes()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();
        slave.Pause();
        slave.Resume();
        slave.Pause();

        var stateBeforeAction = slave.State;

        // Act
        slave.Resume();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Paused));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,
            }));
    }

    [Test]
    public void Resume_Disposed_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();
        slave.Dispose();

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<ObjectDisposedException>(() => slave.Resume());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));

        Assert.That(ex, Has.Message.StartWith("Cannot access a disposed object."));
        Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));
        Assert.That(slave.IsDisposed, Is.True);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public void Resume_ThrowsOnBeforeResuming_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnBeforeResuming = true;

        slave.Start();
        slave.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Resume())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeResuming failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Resume'. 'OnBeforeResuming' has thrown an exception. State is 'Paused'."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
            }));

    }

    [Test]
    public void Resume_ThrowsOnResuming_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnResuming = true;

        slave.Start();
        slave.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Resume())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnResuming failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Resume'. 'OnResuming' has thrown an exception. State will be changed from current 'Resuming' to initial 'Paused'."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
            }));
    }

    [Test]
    public void Resume_ThrowsOnAfterResumed_ThrowsAndSetInRunningState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnAfterResumed = true;

        slave.Start();
        slave.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Resume())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterResumed failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Resume'. 'OnAfterResumed' has thrown an exception. Current state is 'Running' and it will be kept."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,
            }));

    }
}