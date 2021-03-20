using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;
using TauCode.Lab.Infrastructure;
using TauCode.Working.Labor;

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

            // Act
            laborer.Start();

            // Assert
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
        public void Start_Starting_WaitsThenThrowsException()
        {
            // Arrange
            using var laborer = new DemoLaborer
            {
                Logger = new StringLogger(_log),
            };

            var timeMachine = new TimeMachineTimeProviderLab(FakeNow);
            TimeProvider.Override(timeMachine);

            // Act
            laborer.Start();

            // Assert
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
        public async Task Start_Running_ThrowsException()
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
            
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => laborer.Start());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Cannot 'Start' laborer 'Psi' because it is in the 'Running' state."));
        }

        [Test]
        public void Start_Stopping_WaitsThenStarts()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_Pausing_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_Paused_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_Resuming_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_WasStartedStopped_Starts()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_Disposed_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        #endregion

        #region Stop

        [Test]
        public void Stop_Stopped_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Starting_WaitsThenStops()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Running_Stops()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Stopping_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Pausing_WaitsThenStops()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Paused_Stops()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Resuming_WaitsThenStops()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_WasStartedStoppedStarted_Stops()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Stop_Disposed_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        #endregion

        #region Pause

        [Test]
        public void Pause_Stopped_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Starting_WaitsThenPauses()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Running_Pauses()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Stopping_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Pausing_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Paused_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Resuming_WaitsThenPauses()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_WasStartedPausedResumed_Pauses()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Pause_Disposed_ThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
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

