using NUnit.Framework;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Start_Stopped_Starts()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = slave.State;

        // Act
        slave.Start();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped, // initial state

                SlaveState.Stopped, // logged by 'OnBeforeStarting'
                SlaveState.Starting, // logged by 'OnStarting'
                SlaveState.Running, // logged by 'OnAfterStarted'
            }));
    }

    [Test]
    public async Task Start_Starting_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Starting));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Slave state is 'Running'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped, // initial state

                SlaveState.Stopped, // logged by 'OnBeforeStarting'
                SlaveState.Starting, // logged by 'OnStarting'
                SlaveState.Running, // logged by 'OnAfterStarted'
            }));
    }

    [Test]
    public void Start_Running_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Slave state is 'Running'. Slave name is 'Psi'."));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));
        Assert.That(slave.IsDisposed, Is.False);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped, // initial state

                SlaveState.Stopped, // logged by 'OnBeforeStarting'
                SlaveState.Starting, // logged by 'OnStarting'
                SlaveState.Running, // logged by 'OnAfterStarted'
            }));
    }

    [Test]
    public async Task Start_Stopping_WaitsThenStarts()
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
        slave.Start();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

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
                SlaveState.Stopping,
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,
            }));
    }

    [Test]
    public async Task Start_Pausing_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Pausing));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Slave state is 'Paused'. Slave name is 'Psi'."));

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
    public void Start_Paused_ThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Paused));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Slave state is 'Paused'. Slave name is 'Psi'."));

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
    public async Task Start_Resuming_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => slave.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Resuming));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Slave state is 'Running'. Slave name is 'Psi'."));

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
    public void Start_WasStartedStopped_Starts()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Start();
        slave.Stop();

        var stateBeforeAction = slave.State;

        // Act
        slave.Start();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));
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
                SlaveState.Stopping,
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,
            }));

    }

    [Test]
    public void Start_Disposed_ThrowsException()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.Dispose();

        var stateBeforeAction = slave.State;

        // Act
        var ex = Assert.Throws<ObjectDisposedException>(() => slave.Start());

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
            }));
    }

    [Test]
    public void Start_ThrowsOnBeforeStarting_ThrowsAndRemainsInStoppedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnBeforeStarting = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Start())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeStarting failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Start'. 'OnBeforeStarting' has thrown an exception. State is 'Stopped'."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
            }));

    }

    [Test]
    public void Start_ThrowsOnStarting_ThrowsAndRemainsInStoppedState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnStarting = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Start())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnStarting failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Start'. 'OnStarting' has thrown an exception. State will be changed from current 'Starting' to initial 'Stopped'."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
            }));

    }

    [Test]
    public void Start_ThrowsOnAfterStarted_ThrowsAndSetInRunningState()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
        };

        slave.ThrowsOnAfterStarted = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Start())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterStarted failed!"));
        Assert.That(slave.State, Is.EqualTo(SlaveState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoSlave 'Psi') 'Start'. 'OnAfterStarted' has thrown an exception. Current state is 'Running' and it will be kept."));

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,

                SlaveState.Stopped,
                SlaveState.Starting,
                SlaveState.Running,
            }));

    }
}