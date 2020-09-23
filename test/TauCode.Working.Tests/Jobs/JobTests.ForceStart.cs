using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;

namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public partial class JobTests
    {
        [Test]
        public async Task ForceStart_IsEnabledAndNotRunning_Starts()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            var job = jobManager.Create("my-job");

            job.IsEnabled = true;

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await output.WriteAsync("Hello!");
                await Task.Delay(300, token);
            };

            // Act
            job.ForceStart();

            var info1 = job.GetInfo(null);

            await timeMachine.WaitUntilSecondsElapse(start, 0.6);

            var info2 = job.GetInfo(null);

            // Assert
            Assert.That(info1.CurrentRun, Is.Not.Null);
            Assert.That(info1.CurrentRun.Value.StartReason, Is.EqualTo(JobStartReason.Force));

            var run = info2.Runs.Single();
            Assert.That(run.StartReason, Is.EqualTo(JobStartReason.Force));
        }

        [Test]
        public void ForceStart_IsDisabled_ThrowsJobException()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            var job = jobManager.Create("my-job");

            // Act
            var ex = Assert.Throws<JobException>(() => job.ForceStart());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Job 'my-job' is disabled."));
        }

        // todo - already started by force, throws
        [Test]
        public async Task ForceStart_AlreadyStartedByForce_ThrowsTodo()
        {
            throw new NotImplementedException();
        }

        // todo - already started by schedule, throws
        [Test]
        public async Task ForceStart_AlreadyStartedBySchedule_ThrowsTodo()
        {
            throw new NotImplementedException();
        }

        // todo - already started by overridden due time, throws
        [Test]
        public async Task ForceStart_AlreadyStartedByOverriddenDueTime_ThrowsTodo()
        {
            throw new NotImplementedException();
        }

        // todo - long run, get-info changes correctly
        [Test]
        public async Task ForceStart_LongRun_GetInfoChangesCorrectlyWithTime()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void ForceStart_JobIsDisposed_ThrowsJobObjectDisposedException()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            var job = jobManager.Create("my-job");
            job.IsEnabled = true;
            job.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => job.ForceStart());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'my-job' is disposed."));
        }
    }
}
