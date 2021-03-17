using NUnit.Framework;
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

        // todo: Stopped => ok
        // todo: Starting => waits, ex
        // todo: Running => ex
        // todo: Stopping => waits, ok
        // todo: Pausing => waits, ex
        // todo: Paused => ex
        // todo: Resuming => waits, ex
        // todo: Start, stop, start => ok
        // todo: <disposed> => ex

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

        #region Name

        // todo: changed to non-null => ok
        // todo: changed to null => ok
        // todo: <disposed> => can get, can't set

        #endregion

        #region ILogger

        // todo: changed to non-null => ok
        // todo: changed to null => ok
        // todo: <disposed> => can get, can't set

        #endregion
    }
}
