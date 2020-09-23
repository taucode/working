using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;
using TauCode.Working.Schedules;

namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public partial class JobTests
    {
        // todo - was running, then ends => waits and returns true
        [Test]
        public async Task WaitInt_WasRunningThenEnds_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void WaitInt_NegativeArgument_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await output.WriteAsync("Hello!");
                await Task.Delay(TimeSpan.FromSeconds(0.5), token);
            };

            job.IsEnabled = true;

            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, start);

            // Act
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => job.Wait(-1));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("millisecondsTimeout"));
        }

        // todo - was running, then canceled => waits and returns true
        [Test]
        public async Task WaitInt_WasRunningThenCanceled_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then faulted => waits and returns true
        [Test]
        public async Task WaitInt_WasRunningThenFaulted_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then job disposed => waits and returns true
        [Test]
        public async Task WaitInt_WasRunningThenJobIsDisposed_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then job manager disposed => waits and returns true
        [Test]
        public async Task WaitInt_WasRunningThenJobManagerIsDisposed_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, timeout => returns false
        [Test]
        public async Task WaitInt_WasRunningTooLong_WaitsAndReturnsFalse()
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
                await Task.Delay(TimeSpan.FromHours(1), token);
            };

            job.ForceStart();

            // Act

            await timeMachine.WaitUntilSecondsElapse(start, 1.0);

            var gotSignal = job.Wait(1000);

            // Assert
            Assert.That(gotSignal, Is.False);
        }

        [Test]
        public void WaitInt_NotRunning_ReturnsTrueImmediately()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await output.WriteAsync("Hello!");
                await Task.Delay(TimeSpan.FromSeconds(0.5), token);
            };

            // Act
            var gotSignal = job.Wait(10);

            // Assert
            Assert.That(gotSignal, Is.True);
        }

        [Test]
        public void WaitInt_JobIsDisposed_ThrowsJobObjectDisposedException()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await output.WriteAsync("Hello!");
                await Task.Delay(TimeSpan.FromSeconds(0.5), token);
            };

            job.IsEnabled = true;

            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, start);

            job.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => job.Wait(10));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'my-job' is disposed."));
        }
    }
}
