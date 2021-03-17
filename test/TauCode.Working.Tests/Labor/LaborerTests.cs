using NUnit.Framework;
using System;
using TauCode.Working.Labor;

namespace TauCode.Working.Tests.Labor
{
    [TestFixture]
    public class LaborerTests
    {
        #region Constructor

        // todo: happy path on ctor

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

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_Starting_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void Start_Running_WaitsThenThrowsException()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
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
        public void Start_Paused_WaitsThenThrowsException()
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

        // todo: Stopped => ex
        // todo: Starting => waits, ok
        // todo: Running => ok
        // todo: Stopping => waits, ex
        // todo: Pausing => waits, ok
        // todo: Paused => ok
        // todo: Resuming => waits, ex
        // todo: Start, stop, start, stop => ok
        // todo: <disposed> => ex

        #endregion

        #region Pause

        // todo: Stopped => ex
        // todo: Starting => waits, ok
        // todo: Running => ok
        // todo: Stopping => waits, ex
        // todo: Pausing => waits, ex
        // todo: Paused => ex
        // todo: Resuming => waits, ok
        // todo: Start, pause, resume, pause => ok
        // todo: <disposed> => ex

        #endregion

        #region Resume

        // todo: Stopped => ex
        // todo: Starting => waits, ex
        // todo: Running => ex
        // todo: Stopping => waits, ex
        // todo: Pausing => waits, ok
        // todo: Paused => ok
        // todo: Resuming => waits, ex
        // todo: Start, pause, resume, pause, resume => ok
        // todo: <disposed> => ex

        #endregion

        #region Dispose

        // todo: Stopped => ok
        // todo: Starting => waits, ok
        // todo: Running => ok
        // todo: Stopping => waits, ok
        // todo: Pausing => waits, ok
        // todo: Paused => ok
        // todo: Resuming => waits, ok
        // todo: Start, pause, resume, pause, resume => ok
        // todo: <disposed> => does nothing, ok

        #endregion

        #region ILogger

        // todo: changed to non-null => ok
        // todo: changed to null => ok
        // todo: <disposed> => can get, can't set

        #endregion
    }
}
