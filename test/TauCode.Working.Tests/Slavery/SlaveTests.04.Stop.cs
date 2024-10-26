using NUnit.Framework;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Stop_Stopped_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Stop());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Slave state is 'Stopped'. Slave name is 'Psi'."));

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
    public async Task Stop_Starting_WaitsThenStops()
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
        slave.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Starting));

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
                SlaveState.Stopped
            }));
    }

    [Test]
    public void Stop_Running_Stops()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        slave.Start();

        var stateBeforeAction = slave.State;

        // Act
        slave.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));
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
    public async Task Stop_Stopping_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Stop());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Slave state is 'Stopped'. Slave name is 'Psi'."));

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
    public async Task Stop_Pausing_WaitsThenStops()
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
        slave.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Pausing));

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
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public void Stop_Paused_Stops()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
            OnBeforeStartingTimeout = TimeSpan.FromSeconds(1),
        };

        slave.Start();
        slave.Pause();

        var stateBeforeAction = slave.State;

        // Act
        slave.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Paused));

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
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public async Task Stop_Resuming_WaitsThenStops()
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
        slave.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Resuming));

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
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Resuming,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public void Stop_WasStartedStoppedStarted_Stops()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();
        slave.Stop();
        slave.Start();

        var stateBeforeAction = slave.State;

        // Act
        slave.Stop();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));
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

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,

                SlaveState.Running,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public void Stop_Disposed_ThrowsException()
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
        var ex = Assert.Throws<ObjectDisposedException>(() => slave.Stop());

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
    public void Stop_PausedThrowsOnBeforeStopping_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnBeforeStopping = true;

        slave.Start();
        slave.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeStopping failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Stop(bool)'. 'OnBeforeStopping' has thrown an exception. State is 'Paused'."));

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

        slave.ThrowsOnBeforeStopping = false; // let slave get disposed in peace.
    }

    [Test]
    public void Stop_PausedThrowsOnStopping_ThrowsAndRemainsInPausedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnStopping = true;

        slave.Start();
        slave.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnStopping failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Paused));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Stop(bool)'. 'OnStopping' has thrown an exception. State will be changed from current 'Stopping' to initial 'Paused'."));

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
                SlaveState.Stopping,
            }));

        slave.ThrowsOnStopping = false; // let slave get disposed in peace.
    }

    [Test]
    public void Stop_PausedThrowsOnAfterStopped_ThrowsAndSetInStoppedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnAfterStopped = true;

        slave.Start();
        slave.Pause();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterStopped failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Stop(bool)'. 'OnAfterStopped' has thrown an exception. Current state is 'Stopped' and it will be kept."));

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
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));

        slave.ThrowsOnAfterStopped = false; // let slave get disposed in peace.
    }

    [Test]
    public void Stop_RunningThrowsOnBeforeStopping_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnBeforeStopping = true;

        slave.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeStopping failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Stop(bool)'. 'OnBeforeStopping' has thrown an exception. State is 'Running'."));

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

        slave.ThrowsOnBeforeStopping = false; // let slave get disposed in peace.
    }

    [Test]
    public void Stop_RunningThrowsOnStopping_ThrowsAndRemainsInRunningState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnStopping = true;

        slave.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnStopping failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Stop(bool)'. 'OnStopping' has thrown an exception. State will be changed from current 'Stopping' to initial 'Running'."));

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
            }));

        slave.ThrowsOnStopping = false; // let slave get disposed in peace.
    }

    [Test]
    public void Stop_RunningThrowsOnAfterStopped_ThrowsAndSetInStoppedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnAfterStopped = true;

        slave.Start();

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Stop())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterStopped failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Stop(bool)'. 'OnAfterStopped' has thrown an exception. Current state is 'Stopped' and it will be kept."));

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

        slave.ThrowsOnAfterStopped = false; // let slave get disposed in peace.
    }
}