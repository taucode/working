using NUnit.Framework;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    [Test]
    public void Start_Stopped_Starts()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        var stateBeforeAction = worker.State;

        // Act
        worker.Start();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.IsDisposed, Is.False);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped, // initial state

                WorkerState.Stopped, // logged by 'OnBeforeStarting'
                WorkerState.Starting, // logged by 'OnStarting'
                WorkerState.Running, // logged by 'OnAfterStarted'
            }));
    }

    [Test]
    public async Task Start_Starting_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Starting));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.IsDisposed, Is.False);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped, // initial state

                WorkerState.Stopped, // logged by 'OnBeforeStarting'
                WorkerState.Starting, // logged by 'OnStarting'
                WorkerState.Running, // logged by 'OnAfterStarted'
            }));
    }

    [Test]
    public void Start_Running_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'Psi'."));

        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
        Assert.That(worker.IsDisposed, Is.False);

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped, // initial state

                WorkerState.Stopped, // logged by 'OnBeforeStarting'
                WorkerState.Starting, // logged by 'OnStarting'
                WorkerState.Running, // logged by 'OnAfterStarted'
            }));
    }

    [Test]
    public async Task Start_Stopping_WaitsThenStarts()
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
        worker.Start();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

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
                WorkerState.Stopping,
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,
            }));
    }

    [Test]
    public async Task Start_Pausing_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Pausing));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Paused'. Worker name is 'Psi'."));

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
    public void Start_Paused_ThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Paused'. Worker name is 'Psi'."));

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
    public async Task Start_Resuming_WaitsThenThrowsException()
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
        var ex = Assert.Throws<InvalidOperationException>(() => worker.Start());

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'Psi'."));

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
    public void Start_WasStartedStopped_Starts()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Start();
        worker.Stop();

        var stateBeforeAction = worker.State;

        // Act
        worker.Start();

        // Assert
        Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));
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
                WorkerState.Stopping,
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,
            }));

    }

    [Test]
    public void Start_Disposed_ThrowsException()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.Dispose();

        var stateBeforeAction = worker.State;

        // Act
        var ex = Assert.Throws<ObjectDisposedException>(() => worker.Start());

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
            }));
    }

    [Test]
    public void Start_ThrowsOnBeforeStarting_ThrowsAndRemainsInStoppedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnBeforeStarting = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Start())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnBeforeStarting failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Start'. 'OnBeforeStarting' has thrown an exception. State is 'Stopped'."));

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
            }));

    }

    [Test]
    public void Start_ThrowsOnStarting_ThrowsAndRemainsInStoppedState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnStarting = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Start())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnStarting failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Start'. 'OnStarting' has thrown an exception. State will be changed from current 'Starting' to initial 'Stopped'."));

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
            }));

    }

    [Test]
    public void Start_ThrowsOnAfterStarted_ThrowsAndSetInRunningState()
    {
        // Arrange
        using var worker = new DemoWorker(_logger)
        {
            Name = "Psi",
        };

        worker.ThrowsOnAfterStarted = true;

        // Act
        var ex = Assert.Throws<SystemException>(() => worker.Start())!;

        // Assert
        var log = this.CurrentLog;

        Assert.That(ex.Message, Is.EqualTo("OnAfterStarted failed!"));
        Assert.That(worker.State, Is.EqualTo(WorkerState.Running));

        Assert.That(
            log,
            Does.Contain("[VRB] (DemoWorker 'Psi') 'Start'. 'OnAfterStarted' has thrown an exception. Current state is 'Running' and it will be kept."));

        Assert.That(
            worker.History.ToArray(),
            Is.EquivalentTo(new[]
            {
                WorkerState.Stopped,

                WorkerState.Stopped,
                WorkerState.Starting,
                WorkerState.Running,
            }));

    }
}