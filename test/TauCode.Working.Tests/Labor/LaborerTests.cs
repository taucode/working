using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;
using TauCode.Lab.Infrastructure;
using TauCode.Working.Labor;
using TauCode.Working.Labor.Exceptions;

// todo: need those time machines inside tests? they're confusing since not being used.
namespace TauCode.Working.Tests.Labor
{
    [TestFixture]
    public class LaborerTests
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
            ILaborer laborer = new DemoLaborer();

            // Assert
            Assert.That(laborer.Name, Is.Null);
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);
            Assert.That(laborer.Logger, Is.Null);
        }

        #endregion

        #region Name

        // todo: changed to non-null => Ok
        // todo: changed to null => ok
        // todo: <disposed> => name not changed, can read, not write

        #endregion

        #region Start

        [Test]
        public void Start_Stopped_Starts()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Start();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running
                }));
        }

        [Test]
        public async Task Start_Starting_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var startTask = new Task(() => laborer.Start());
            startTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Starting));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Laborer state is 'Running'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running
                }));
        }

        [Test]
        public void Start_Running_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Laborer state is 'Running'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running
                }));
        }

        [Test]
        public async Task Start_Stopping_WaitsThenStarts()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stopTask = new Task(() => laborer.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Start();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                }));
        }

        [Test]
        public async Task Start_Pausing_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnPausingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var pauseTask = new Task(() => laborer.Pause());
            pauseTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Pausing));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Laborer state is 'Paused'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public void Start_Paused_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Paused));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Laborer state is 'Paused'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public async Task Start_Resuming_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();

            var resumeTask = new Task(() => laborer.Resume());
            resumeTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Resuming));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Laborer state is 'Running'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Resuming,
                    LaborerState.Running,
                }));
        }

        [Test]
        public void Start_WasStartedStopped_Starts()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            laborer.Start();
            laborer.Stop();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Start();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                }));

        }

        [Test]
        public void Start_Disposed_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Dispose();

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => laborer.Start());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Start' because laborer is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                }));
        }

        #endregion

        #region Stop

        [Test]
        public void Stop_Stopped_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Starting_WaitsThenStops()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var startTask = new Task(() => laborer.Start());
            startTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Stop();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Starting));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped
                }));
        }

        [Test]
        public void Stop_Running_Stops()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Stop();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Stopping_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stopTask = new Task(() => laborer.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Pausing_WaitsThenStops()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnPausingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var pauseTask = new Task(() => laborer.Pause());
            pauseTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Stop();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Pausing));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public void Stop_Paused_Stops()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Stop();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Paused));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Stop_Resuming_WaitsThenStops()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();
            
            var resumeTask = new Task(() => laborer.Resume());
            resumeTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Stop();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Resuming));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Resuming,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public void Stop_WasStartedStoppedStarted_Stops()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            laborer.Start();
            laborer.Stop();
            laborer.Start();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Stop();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public void Stop_Disposed_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Dispose();

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => laborer.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Stop' because laborer is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        #endregion

        #region Pause

        [Test]
        public void Pause_Stopped_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Pause_Starting_WaitsThenPauses()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStartingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);
            
            var startTask = new Task(() => laborer.Start());
            startTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Pause();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Starting));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public void Pause_Running_Pauses()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Pause();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public async Task Pause_Stopping_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stopTask = new Task(() => laborer.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Pause_Pausing_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnStoppingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();

            var stopTask = new Task(() => laborer.Stop());
            stopTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Stop());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public void Pause_Paused_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Paused));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Pause'. Laborer state is 'Paused'. Laborer name is 'Psi'."));
            Assert.That(ex, Has.Property(nameof(InvalidLaborerOperationException.LaborerName)).EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public async Task Pause_Resuming_WaitsThenPauses()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
                OnResumingTimeout = TimeSpan.FromSeconds(1),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();
            
            var resumeTask = new Task(() => laborer.Resume());
            resumeTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Pause();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Resuming));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Resuming,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public void Pause_WasStartedPausedResumed_Pauses()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            laborer.Start();
            laborer.Pause();
            laborer.Resume();
            
            var stateBeforeAction = laborer.State;

            // Act
            laborer.Pause();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.IsDisposed, Is.False);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Resuming,
                    LaborerState.Running,
                    LaborerState.Pausing,
                    LaborerState.Paused,
                }));
        }

        [Test]
        public void Pause_Disposed_ThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Dispose();

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => laborer.Pause());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Pause' because laborer is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                    LaborerState.Starting,
                    LaborerState.Running,
                    LaborerState.Stopping,
                    LaborerState.Stopped,
                }));
        }

        #endregion

        #region Resume

        [Test]
        public void Resume_Stopped_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Starting_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Running_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Stopping_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Pausing_WaitsThenResumes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Paused_Resumes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Resuming_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_WasStartedPausedResumedPaused_Resumes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Resume_Disposed_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        #endregion

        #region Dispose

        [Test]
        public void Dispose_Stopped_Disposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Starting_WaitsThenDisposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Running_Disposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Stopping_WaitsThenDisposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Pausing_WaitsThenDisposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Paused_Disposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Resuming_WaitsThenDisposes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_WasStartedPausedResumedPaused_Resumes()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Dispose_Disposed_DoesNothing()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }
        #endregion

        #region Logger

        [Test]
        public void Logger_NoArguments_SetCorrectly()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Logger_Disposed_CanBeGot()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        #endregion
    }
}

