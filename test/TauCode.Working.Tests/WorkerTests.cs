using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;
using TauCode.Lab.Infrastructure;
using TauCode.Working.Exceptions;

// todo: need those time machines inside tests? they're confusing since not being used.
namespace TauCode.Working.Tests
{
    [TestFixture]
    public class WorkerTests
    {
        private StringBuilder _log;
        private static readonly DateTimeOffset FakeNow = "2021-01-01Z".ToUtcDateOffset();

        [SetUp]
        public void SetUp()
        {
            _log = new StringBuilder();
            TimeProvider.Reset();
        }

        #region Constructor

        [Test]
        public void Constructor_NoArguments_RunsOk()
        {
            // Arrange

            // Act
            IWorker worker = new DemoWorker();

            // Assert
            Assert.That(worker.Name, Is.Null);
            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.False);
            Assert.That(worker.Logger, Is.Null);
        }

        #endregion

        #region Name

        [Test]
        public void Name_NoArguments_SetAndGot()
        {
            // Arrange
            var worker = new DemoWorker();

            // Act
            worker.Name = "some_name";
            var name1 = worker.Name;

            worker.Name = null;
            var name2 = worker.Name;

            // Assert
            Assert.That(name1, Is.EqualTo("some_name"));
            Assert.That(name2, Is.Null);
        }

        [Test]
        public void Name_Disposed_CanBeGot()
        {
            // Arrange
            var worker = new DemoWorker
            {
                Name = "some_name",
            };

            worker.Dispose();

            // Act
            var gotName = worker.Name;
            var ex = Assert.Throws<ObjectDisposedException>(() => worker.Name = null);

            // Assert
            Assert.That(gotName, Is.EqualTo("some_name"));
            Assert.That(ex, Has.Message.StartsWith("Cannot perform operation 'set Name' because worker is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("some_name"));
        }

        #endregion

        #region Start

        [Test]
        public void Start_Stopped_Starts()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running
                }));
        }

        [Test]
        public async Task Start_Starting_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var startTask = new Task(() => worker.Start());
            startTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Starting));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running
                }));
        }

        [Test]
        public void Start_Running_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running
                }));
        }

        [Test]
        public async Task Start_Stopping_WaitsThenStarts()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                }));
        }

        [Test]
        public async Task Start_Pausing_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnPausingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var pauseTask = new Task(() => worker.Pause());
            pauseTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Pausing));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Paused'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public void Start_Paused_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Pause();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Paused'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public async Task Start_Resuming_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Pause();

            var resumeTask = new Task(() => worker.Resume());
            resumeTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                }));
        }

        [Test]
        public void Start_WasStartedStopped_Starts()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                }));

        }

        [Test]
        public void Start_Disposed_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Dispose();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => worker.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Start' because worker is disposed."));
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

        #endregion

        #region Stop

        [Test]
        public void Stop_Stopped_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

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
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped
                }));
        }

        [Test]
        public void Stop_Running_Stops()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Stopping_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var stopTask = new Task(() => worker.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Pausing_WaitsThenStops()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnPausingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Stop_Paused_Stops()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Resuming_WaitsThenStops()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Stop_WasStartedStoppedStarted_Stops()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Stop_Disposed_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Dispose();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => worker.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Stop' because worker is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.True);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        #endregion

        #region Pause

        [Test]
        public void Pause_Stopped_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

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
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public void Pause_Running_Pauses()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public async Task Pause_Stopping_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var stopTask = new Task(() => worker.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Pause_Pausing_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var stopTask = new Task(() => worker.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Pause_Paused_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Pause();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Paused));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Worker state is 'Paused'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Paused));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public async Task Pause_Resuming_WaitsThenPauses()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public void Pause_WasStartedPausedResumed_Pauses()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                }));
        }

        [Test]
        public void Pause_Disposed_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Dispose();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => worker.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Pause' because worker is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.True);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        #endregion

        #region Resume

        [Test]
        public void Resume_Stopped_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

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
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var startTask = new Task(() => worker.Start());
            startTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Starting));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Running'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running
                }));
        }

        [Test]
        public void Resume_Running_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Running));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Running'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running
                }));
        }

        [Test]
        public async Task Resume_Stopping_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();

            var stopTask = new Task(() => worker.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Stopped'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Resume_Pausing_WaitsThenResumes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnPausingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                }));
        }

        [Test]
        public void Resume_Paused_Resumes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                }));
        }

        [Test]
        public async Task Resume_Resuming_WaitsThenThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Pause();

            var resumeTask = new Task(() => worker.Resume());
            resumeTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<InvalidWorkerOperationException>(() => worker.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Resuming));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Worker state is 'Running'. Worker name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidWorkerOperationException.WorkerName)).EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Running));
            Assert.That(worker.IsDisposed, Is.False);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                }));
        }

        [Test]
        public void Resume_WasStartedPausedResumedPaused_Resumes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                }));
        }

        [Test]
        public void Resume_Disposed_ThrowsException()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            worker.Start();
            worker.Dispose();

            var stateBeforeAction = worker.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => worker.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(WorkerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Resume' because worker is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

            Assert.That(worker.State, Is.EqualTo(WorkerState.Stopped));
            Assert.That(worker.IsDisposed, Is.True);

            Assert.That(
                worker.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    WorkerState.Stopped,
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        #endregion

        #region Dispose

        [Test]
        public void Dispose_Stopped_Disposes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Dispose_Running_Disposes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Dispose_Stopping_WaitsThenDisposes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Dispose_Pausing_WaitsThenDisposes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnPausingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Dispose_Paused_Disposes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public async Task Dispose_Resuming_WaitsThenDisposes()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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
                    WorkerState.Starting,
                    WorkerState.Running,
                    WorkerState.Pausing,
                    WorkerState.Paused,
                    WorkerState.Resuming,
                    WorkerState.Running,
                    WorkerState.Stopping,
                    WorkerState.Stopped,
                }));
        }

        [Test]
        public void Dispose_Disposed_DoesNothing()
        {
            // Arrange
            using var worker = new DemoWorker
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

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

        #endregion

        #region Logger

        [Test]
        public void Logger_NoArguments_SetCorrectly()
        {
            // Arrange
            var worker = new DemoWorker();
            var logger = new Mock<ILogger>().Object;

            // Act
            worker.Logger = logger;
            var logger1 = worker.Logger;

            worker.Logger = null;
            var logger2 = worker.Logger;

            // Assert
            Assert.That(logger1, Is.SameAs(logger));
            Assert.That(logger2, Is.Null);
        }

        [Test]
        public void Logger_Disposed_CanBeGot()
        {
            // Arrange
            var worker = new DemoWorker
            {
                Name = "Psi",
            };
            var logger = new Mock<ILogger>().Object;
            worker.Logger = logger;

            worker.Dispose();

            // Act
            var gotLogger = worker.Logger;
            var ex = Assert.Throws<ObjectDisposedException>(() => worker.Logger = null);

            // Assert
            Assert.That(gotLogger, Is.SameAs(logger));
            Assert.That(ex, Has.Message.StartsWith("Cannot perform operation 'set Logger' because worker is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));
        }

        #endregion
    }
}
