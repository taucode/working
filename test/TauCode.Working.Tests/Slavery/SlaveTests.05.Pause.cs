using NUnit.Framework;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Pause_Stopped_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Slave state is 'Stopped'. Slave name is 'Psi'."));

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
    public async Task Pause_Starting_WaitsThenPauses()
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
        slave.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Starting));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));
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
            }));
    }

    [Test]
    public void Pause_Running_Pauses()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        slave.Start();

        var stateBeforeAction = slave.State;

        // Act
        slave.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));
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
            }));
    }

    [Test]
    public async Task Pause_Stopping_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Slave state is 'Stopped'. Slave name is 'Psi'."));

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
    public async Task Pause_Pausing_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Slave state is 'Stopped'. Slave name is 'Psi'."));

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
    public void Pause_Paused_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();
        slave.Pause();

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Pause());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Paused));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Slave state is 'Paused'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));
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
            }));
    }

    [Test]
    public async Task Pause_Resuming_WaitsThenPauses()
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
        slave.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Resuming));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));
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
            }));
    }

    [Test]
    public void Pause_WasStartedPausedResumed_Pauses()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();
        slave.Pause();
        slave.Resume();

        var stateBeforeAction = slave.State;

        // Act
        slave.Pause();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));
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
            }));
    }

    [Test]
    public void Pause_Disposed_ThrowsException()
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
        var ex = Assert.Throws<ObjectDisposedException>(() => slave.Pause());

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
    public void Pause_ThrowsOnBeforePausing_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnBeforePausing = true;

        slave.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Pause())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforePausing failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Pause'. 'OnBeforePausing' has thrown an exception. State is 'Running'."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
            }));

    }

    [Test]
    public void Pause_ThrowsOnPausing_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnPausing = true;

        slave.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Pause())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnPausing failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Pause'. 'OnPausing' has thrown an exception. State will be changed from current 'Pausing' to initial 'Running'."));

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
            }));
    }

    [Test]
    public void Pause_ThrowsOnAfterPaused_ThrowsAndSetInPausedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnAfterPaused = true;

        slave.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Pause())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterPaused failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Pause'. 'OnAfterPaused' has thrown an exception. Current state is 'Paused' and it will be kept."));

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
            }));

    }
}