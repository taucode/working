using NUnit.Framework;
using TauCode.Working.Slavery;

namespace TauCode.Working.Tests.Slavery;

[TestFixture]
public partial class SlaveTests
{
    [Test]
    public void Dispose_Stopped_Disposes()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        var stateBeforeAction = slave.State;

        // Act
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));
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
    public async Task Dispose_Starting_WaitsThenDisposes()
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
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

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
    public void Dispose_Running_Disposes()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        slave.Start();

        var stateBeforeAction = slave.State;

        // Act
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Running));
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
    public async Task Dispose_Stopping_WaitsThenDisposes()
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
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopping));

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
    public async Task Dispose_Pausing_WaitsThenDisposes()
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
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Pausing));

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
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public void Dispose_Paused_Disposes()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        slave.Start();
        slave.Pause();

        var stateBeforeAction = slave.State;

        // Act
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Paused));
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
                SlaveState.Pausing,
                SlaveState.Paused,

                SlaveState.Paused,
                SlaveState.Stopping,
                SlaveState.Stopped,
            }));
    }

    [Test]
    public async Task Dispose_Resuming_WaitsThenDisposes()
    {
        // Arrange
        using var slave = new DemoSlave(_logger)
        {
            Name = "Psi",
            OnResumingTimeout = TimeSpan.FromSeconds(1),
        };

        slave.Start();
        slave.Pause();

        var pauseTask = new Task(() => slave.Resume());
        pauseTask.Start();
        await Task.Delay(100); // let task start

        var stateBeforeAction = slave.State;

        // Act
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Resuming));

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
    public void Dispose_Disposed_DoesNothing()
    {
        // Arrange
        using var slave = new DemoSlave(logger: _logger);

        slave.Dispose();

        var stateBeforeAction = slave.State;

        // Act
        slave.Dispose();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(SlaveState.Stopped));
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
    public void Dispose_ThrowsOnDisposing_ThrowsAndDisposes()
    {
        // Arrange
        using var slave = new DemoSlave(_logger);

        slave.ThrowsOnAfterDisposed = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => slave.Dispose())!;

        // Assert
        Assert.That(ex.Message, Is.EqualTo("OnAfterDisposed failed!"));

        Assert.That(slave.State, Is.EqualTo(SlaveState.Stopped));
        Assert.That(slave.IsDisposed, Is.True);

        Assert.That(
            slave.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                SlaveState.Stopped,
            }));

        slave.ThrowsOnAfterDisposed = false; // let slave get disposed in peace.
    }
}