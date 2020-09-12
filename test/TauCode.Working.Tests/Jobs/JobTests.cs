using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;
using TauCode.Working.Schedules;

namespace TauCode.Working.Tests.Jobs
{
    // todo: dispose resources in all ut-s
    [TestFixture]
    public class JobTests
    {
        [SetUp]
        public void SetUp()
        {
            TimeProvider.Reset();
        }

        #region IJob.Schedule

        /// <summary>
        /// Test script:
        /// Create a job and check its schedule is 'Never'
        /// </summary>
        [Test]
        public void Schedule_JustCreatedJob_ReturnsNeverSchedule()
        {
            // Arrange
            var mgr = new JobManager();
            mgr.Start();
            var job = mgr.Create("my-job");

            // Act
            var schedule = job.Schedule;

            // Assert
            Assert.That(schedule, Is.Not.Null);
            Assert.That(schedule.GetType().FullName, Is.EqualTo("TauCode.Working.Schedules.NeverSchedule"));

            mgr.Dispose();
        }

        #endregion


        //====================================================================================

        [Test]
        public void GetInfo_NoArguments_ReturnsJobInfo()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            var name = "job1";
            var job = jobManager.Create(name);

            // Act
            var info = job.GetInfo(null);

            // Assert
            Assert.That(info.Name, Is.EqualTo(name));
            Assert.That(info.CurrentRun, Is.Null);

            Assert.That(info.DueTimeInfo.Type, Is.EqualTo(DueTimeType.BySchedule));
            Assert.That(info.DueTimeInfo.DueTime, Is.EqualTo(JobExtensions.Never));

            //Assert.That(info.IsEnabled, Is.True);
            Assert.That(info.RunCount, Is.Zero);
            Assert.That(info.Runs, Is.Empty);
        }

        [Test]
        public void ManualChangeDueTime_NotNull_DueTimeIsChanged()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            var job = jobManager.Create("job1");

            var now = "2020-09-11".ToExactUtcDate().AddHours(11);
            TimeProvider.Override(now);

            var manualDueTime = "2020-10-12".ToExactUtcDate().AddHours(1);

            // Act
            job.OverrideDueTime(manualDueTime);

            // Assert
            var info = job.GetInfo(null);
            Assert.That(info.DueTimeInfo.Type, Is.EqualTo(DueTimeType.Overridden));
            Assert.That(info.DueTimeInfo.DueTime, Is.EqualTo(manualDueTime));
        }

        [Test]
        public async Task ForceStart_NotStarted_RunsSuccessfully()
        {
            // Arrange
            var now = "2020-09-11".ToExactUtcDate();
            TimeProvider.Override(now);

            IJobManager jobManager = new JobManager();
            jobManager.Start();
            var job = jobManager.Create("job1");

            // Act
            job.ForceStart();
            await Task.Delay(100); // allow job to complete

            // Assert
            var info = job.GetInfo(null);

            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.RunCount, Is.EqualTo(1));

            Assert.That(info.DueTimeInfo.Type, Is.EqualTo(DueTimeType.BySchedule));
            Assert.That(info.DueTimeInfo.IsNever(), Is.True);

            Assert.That(info.Runs, Has.Count.EqualTo(1));
            var run = info.Runs.Single();

            Assert.That(run.Index, Is.EqualTo(0));
            Assert.That(run.StartReason, Is.EqualTo(StartReason.Force));

            Assert.That(run.DueTimeInfo.Type, Is.EqualTo(DueTimeType.BySchedule));
            Assert.That(run.DueTimeInfo.IsNever(), Is.True);

            Assert.That(run.StartTime, Is.EqualTo(now));
            Assert.That(run.EndTime, Is.EqualTo(now));
            Assert.That(run.Status, Is.EqualTo(JobRunStatus.Succeeded));
            Assert.That(run.Output, Does.StartWith("Warning: usage of default idle routine."));
            Assert.That(run.Exception, Is.Null);
        }

        [Test]
        public async Task SetSchedule_ValidValue_SetsSchedule()
        {
            // Arrange
            var now = "2020-09-11".ToExactUtcDate().AddHours(3);
            TimeProvider.Override(now);

            IJobManager jobManager = new JobManager();
            jobManager.Start();

            var name = "job1";
            var job = jobManager.Create(name);

            var writer = new StringWriterWithEncoding(Encoding.UTF8);
            job.Output = writer;

            // Act
            var newSchedule = new SimpleSchedule(SimpleScheduleKind.Minute, 1, now);
            job.UpdateSchedule(newSchedule);
            job.Routine = (parameter, tracker, output, token) =>
            {
                output.Write("Hello!");
                return Task.CompletedTask;
            };

            var finished = now.AddMinutes(1).AddMilliseconds(1);
            await Task.Run(async () =>
            {
                TimeProvider.Override(finished); // pretend due time come
                jobManager.DebugPulseJobManager(); // this will break Vice's vacation
                await Task.Delay(100); // should be enough for Routine to complete
            });

            // Assert
            Assert.That(writer.ToString(), Is.EqualTo("Hello!"));
            Assert.That(job.Schedule, Is.SameAs(newSchedule));
        }

        // todo: IJob.Schedule
        // - initially, equals to Never
        // - after was set, changes to new
        // - after was disposed, throws exception.

        // todo: IJob.UpdateSchedule
        // - 1. just created, 2. called => changes, due time changes
        // - 1. forcibly started 2. called => schedule changes, but returns 'false'; job due time changes; current run's due time not changed; after completion, run logs shows 'old' due time, and jobs' due time is next by the schedule.
        // - like in previous, but started not forcibly but by schedule
        // - if due time overridden, throws an exception
        // - after was disposed, throws exception.
    }
}
