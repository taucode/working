using NUnit.Framework;
using System;
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

        [Test]
        public void GetSchedule_JustCreatedJob_ReturnsNeverSchedule()
        {
            // Arrange
            IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            var schedule = job.Schedule;

            // Assert
            Assert.That(schedule, Is.Not.Null);
            Assert.That(schedule.GetType().FullName, Is.EqualTo("TauCode.Working.Schedules.NeverSchedule"));

            jobManager.Dispose();
        }

        #endregion


        //====================================================================================

        [Test]
        public void GetInfo_NoArguments_ReturnsJobInfo()
        {
            // Arrange
            IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var name = "job1";
            var job = jobManager.Create(name);

            // Act
            var info = job.GetInfo(null);

            // Assert
            Assert.That(info.CurrentRun, Is.Null);
            throw new NotImplementedException();
            //Assert.That(info.NextDueTimeInfo.Type, Is.EqualTo(DueTimeType.BySchedule));
            //Assert.That(info.NextDueTimeInfo.DueTime, Is.EqualTo(JobExtensions.Never));

            ////Assert.That(info.IsEnabled, Is.True);
            //Assert.That(info.RunCount, Is.Zero);
            //Assert.That(info.Runs, Is.Empty);
        }

        [Test]
        public void ManualChangeDueTime_NotNull_DueTimeIsChanged()
        {
            // Arrange
            IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("job1");

            var now = "2020-09-11Z".ToUtcDayOffset().AddHours(11);
            TimeProvider.Override(now);

            var manualDueTime = "2020-10-12Z".ToUtcDayOffset().AddHours(1);

            // Act
            job.OverrideDueTime(manualDueTime);

            // Assert
            var info = job.GetInfo(null);
            //Assert.That(info.NextDueTimeInfo.Type, Is.EqualTo(DueTimeType.Overridden));
            //Assert.That(info.NextDueTimeInfo.DueTime, Is.EqualTo(manualDueTime));
            throw new NotImplementedException();
        }

        [Test]
        public async Task ForceStart_NotStarted_RunsSuccessfully()
        {
            // Arrange
            var now = "2020-09-11Z".ToUtcDayOffset();
            TimeProvider.Override(now);

            IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("job1");

            // Act
            job.ForceStart();
            await Task.Delay(100); // allow job to complete

            // Assert
            var info = job.GetInfo(null);

            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.RunCount, Is.EqualTo(1));

            throw new NotImplementedException();
            //Assert.That(info.NextDueTimeInfo.Type, Is.EqualTo(DueTimeType.BySchedule));
            //Assert.That(info.NextDueTimeInfo.IsNever(), Is.True);

            //Assert.That(info.Runs, Has.Count.EqualTo(1));
            //var run = info.Runs.Single();

            //Assert.That(run.Index, Is.EqualTo(0));
            //Assert.That(run.StartReason, Is.EqualTo(JobStartReason.Force));

            //Assert.That(run.DueTimeInfo.Type, Is.EqualTo(DueTimeType.BySchedule));
            //Assert.That(run.DueTimeInfo.IsNever(), Is.True);

            //Assert.That(run.StartTime, Is.EqualTo(now));
            //Assert.That(run.EndTime, Is.EqualTo(now));
            //Assert.That(run.Status, Is.EqualTo(JobRunStatus.Succeeded));
            //Assert.That(run.Output, Does.StartWith("Warning: usage of default idle routine."));
            //Assert.That(run.Exception, Is.Null);
        }

        [Test]
        public async Task SetSchedule_ValidValue_SetsSchedule()
        {
            // Arrange
            var now = "2020-09-11Z".ToUtcDayOffset().AddHours(3);
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();

            var name = "job1";
            var job = jobManager.Create(name);

            var writer = new StringWriterWithEncoding(Encoding.UTF8);
            job.Output = writer;

            // Act
            var newSchedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now.AddSeconds(2));
            job.Schedule = newSchedule;
            job.Routine = (parameter, tracker, output, token) =>
            {
                output.Write("Hello!");
                return Task.CompletedTask;
            };

            await Task.Delay(2500);

            // Assert
            Assert.That(writer.ToString(), Is.EqualTo("Hello!"));
            Assert.That(job.Schedule, Is.SameAs(newSchedule));
        }

        //====================================================================================

        // todo: IJob.Name
        // - just after created, equals to that with which was created
        // - if enabled, disabled, running, not running, disposed - still the same

        // todo: IJob.IsEnabled
        // - initially, false
        // - when changed to true, changes.
        // - when changed to false, changes.
        // - when disposed, still can be read.
        // - when disposed, cannot be set - throws.

        // todo: IJob.Schedule
        // - initially, equals to Never
        // - cannot be set to null, throws.
        // - after was set, changes to new, be IJob instance enabled or disabled.
        // - after was set, is reflected im get-info
        // - after was set and IJob started => reflected in current-run, and get-info is updated to next calculated.
        // - after was set and IJob started and completed => reflected in old runs.
        // - after was set and IJob started and canceled => reflected in old runs.
        // - after was set and IJob started and faulted => reflected in old runs.
        // - after was set, discards overridden due time.
        // - after was set, discards previous schedule's due time.
        // - after was disposed, equals to last.
        // - after was disposed, cannot be set, throws.
        // - if schedule produces strange results (throws, date before 'now', date after 'never') then sets due time to 'never' and adds a virtual run entry describing the problem.


        // todo: IJob.Routine
        // - initially, not null.
        // - cannot be set to null, throws.
        // - cannot be set if job runs (by schedule)
        // - cannot be set if job runs (by overridden due time)
        // - cannot be set if job runs (by force)
        // - when set, updated to new value, regardless of enabled or disabled
        // - when set after run completed, afterwards runs with new routine.
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        // todo: IJob.Parameter
        // - initially, null
        // - can be set to any value including null (if not running), be it enabled or disabled.
        // - cannot be set if job runs (by schedule)
        // - cannot be set if job runs (by overridden due time)
        // - cannot be set if job runs (by force)
        // - when set after run completed, afterwards runs with new parameter value.
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        // todo: IJob.ProgressTracker
        // - initially, null
        // - can be set to any value including null (if not running), be it enabled or disabled.
        // - cannot be set if job runs (by schedule)
        // - cannot be set if job runs (by overridden due time)
        // - cannot be set if job runs (by force)
        // - when set after run completed, afterwards runs with new progress tracker.
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        // todo: IJob.Output
        // - initially, null
        // - can be set to any value including null (if not running), be it enabled or disabled.
        // - cannot be set if job runs (by schedule)
        // - cannot be set if job runs (by overridden due time)
        // - cannot be set if job runs (by force)
        // - when set after run completed, afterwards runs with new output.
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        // todo: IJob.GetInfo
        // - IJob just created => returns predictable result
        // - when arg is null, returns complete run log
        // - when arg < 0, throws
        // - when arg == 0, doesn't return log (empty)
        // - when arg > log length, returns full log.
        // - run several times (by force, schedule, overridden due time) and check the runs.
        // - if routine runs for long, GetInfo shows correct due time during the routine run. after routine ends, due time is also valid. NB: Vice should log due time changes while job's long run.
        // - after disposed, still can be called.

        // todo: IJob.OverrideDueTime
        // - when set to non-null value => reflected in get-info
        // - when set to null => defaults to schedule, which reflects in get-info
        // - if arg is < 'now' => throws, nothing happens to schedule
        // - when set and that moment comes => starts and defaults to schedule, check get-info.
        // - when set to non-null during run => set to next due time, but not current run.
        // - when set to non-null during run and that run turned out too long => will not have effect at the very end. (+logs)
        // - after disposed => cannot be set, but remains forever, even after its due time comes.
        // - if IJob is disabled, won't run whichever  values do you set.

        // todo: IJob.ForceStart
        // - happy path, get-info
        // - already running, throws
        // - long run, due time evolution + logs
        // - disposed, throws.

        // todo: IJob.Cancel
        // - was running => cancelled, returns true + logs
        // - was stopped => returns false
        // - disposed => throws

        // todo: IJob.IsDisposed
        // - obvious

        // todo: IJob.Dispose
        // - dispose not running IJob, checks
        // - dispose running IJob, checks
        // - dispose disposed, no problem.

    }
}
