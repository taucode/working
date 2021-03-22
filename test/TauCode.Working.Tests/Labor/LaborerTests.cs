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

        [Test]
        public void Name_NoArguments_SetAndGot()
        {
            // Arrange
            var laborer = new DemoLaborer();

            // Act
            laborer.Name = "some_name";
            var name1 = laborer.Name;

            laborer.Name = null;
            var name2 = laborer.Name;

            // Assert
            Assert.That(name1, Is.EqualTo("some_name"));
            Assert.That(name2, Is.Null);
        }

        [Test]
        public void Name_Disposed_CanBeGot()
        {
            // Arrange
            var laborer = new DemoLaborer
            {
                Name = "some_name",
            };

            laborer.Dispose();

            // Act
            var gotName = laborer.Name;
            var ex = Assert.Throws<ObjectDisposedException>(() => laborer.Name = null);

            // Assert
            Assert.That(gotName, Is.EqualTo("some_name"));
            Assert.That(ex, Has.Message.StartsWith("Cannot perform operation 'set Name' because laborer is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("some_name"));
        }

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
            using var laborer = new DemoLaborer
            {
                Name = "Psi",
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            var stateBeforeAction = laborer.State;

            // Act
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
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
        public async Task Resume_Starting_WaitsThenThrowsException()
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
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Starting));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Laborer state is 'Running'. Laborer name is 'Psi'."));
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
        public void Resume_Running_ThrowsException()
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
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Laborer state is 'Running'. Laborer name is 'Psi'."));
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
        public async Task Resume_Stopping_WaitsThenThrowsException()
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
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Laborer state is 'Stopped'. Laborer name is 'Psi'."));
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
        public async Task Resume_Pausing_WaitsThenResumes()
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
            laborer.Resume();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Pausing));

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
        public void Resume_Paused_Resumes()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Resume();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Paused));
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
        public async Task Resume_Resuming_WaitsThenThrowsException()
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
            var ex = Assert.Throws<InvalidLaborerOperationException>(() => laborer.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Resuming));

            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Resume'. Laborer state is 'Running'. Laborer name is 'Psi'."));
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
        public void Resume_WasStartedPausedResumedPaused_Resumes()
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
            laborer.Pause();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Resume();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Paused));
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
                    LaborerState.Pausing,
                    LaborerState.Paused,
                    LaborerState.Resuming,
                    LaborerState.Running,
                }));
        }

        [Test]
        public void Resume_Disposed_ThrowsException()
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
            var ex = Assert.Throws<ObjectDisposedException>(() => laborer.Resume());

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));

            Assert.That(ex, Has.Message.StartsWith($"Cannot perform operation 'Resume' because laborer is disposed."));
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

        #region Dispose

        [Test]
        public void Dispose_Stopped_Disposes()
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
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

            Assert.That(
                laborer.History.ToArray(),
                Is.EquivalentTo(new[]
                {
                    LaborerState.Stopped,
                }));
        }

        [Test]
        public async Task Dispose_Starting_WaitsThenDisposes()
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
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

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

        [Test]
        public void Dispose_Running_Disposes()
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
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Running));
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

        [Test]
        public async Task Dispose_Stopping_WaitsThenDisposes()
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
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopping));

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

        [Test]
        public async Task Dispose_Pausing_WaitsThenDisposes()
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
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Pausing));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

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
        public void Dispose_Paused_Disposes()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Start();
            laborer.Pause();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Paused));
            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

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
        public async Task Dispose_Resuming_WaitsThenDisposes()
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

            var pauseTask = new Task(() => laborer.Resume());
            pauseTask.Start();
            await Task.Delay(100); // let task start

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Resuming));

            Assert.That(laborer.State, Is.EqualTo(LaborerState.Stopped));
            Assert.That(laborer.IsDisposed, Is.True);

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
        public void Dispose_Disposed_DoesNothing()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            laborer.Dispose();

            var stateBeforeAction = laborer.State;

            // Act
            laborer.Dispose();

            // Assert
            Assert.That(stateBeforeAction, Is.EqualTo(LaborerState.Stopped));
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

        #region Logger

        [Test]
        public void Logger_NoArguments_SetCorrectly()
        {
            // Arrange
            var laborer = new DemoLaborer();
            var logger = new Mock<ILogger>().Object;

            // Act
            laborer.Logger = logger;
            var logger1 = laborer.Logger;

            laborer.Logger = null;
            var logger2 = laborer.Logger;

            // Assert
            Assert.That(logger1, Is.SameAs(logger));
            Assert.That(logger2, Is.Null);
        }

        [Test]
        public void Logger_Disposed_CanBeGot()
        {
            // Arrange
            var laborer = new DemoLaborer
            {
                Name = "Psi",
            };
            var logger = new Mock<ILogger>().Object;
            laborer.Logger = logger;

            laborer.Dispose();

            // Act
            var gotLogger = laborer.Logger;
            var ex = Assert.Throws<ObjectDisposedException>(() => laborer.Logger = null);

            // Assert
            Assert.That(gotLogger, Is.SameAs(logger));
            Assert.That(ex, Has.Message.StartsWith("Cannot perform operation 'set Logger' because laborer is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("Psi"));
        }

        #endregion
    }
}
