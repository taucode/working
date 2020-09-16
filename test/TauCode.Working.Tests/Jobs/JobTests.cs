﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;
using TauCode.Working.Schedules;

namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public class JobTests
    {
        [SetUp]
        public void SetUp()
        {
            TimeProvider.Reset();
        }

        #region IJob.Name

        [Test]
        public void Name_JustCreatedJob_ReturnsValidName()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            var name = job.Name;

            // Assert
            Assert.That(name, Is.EqualTo("my-job"));
        }

        [Test]
        public void Name_JobIsEnabled_ReturnsValidName()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.IsEnabled = true;

            // Act
            var name = job.Name;

            // Assert
            Assert.That(name, Is.EqualTo("my-job"));
        }

        [Test]
        public void Name_JobIsDisabled_ReturnsValidName()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.IsEnabled = true;

            // Act
            var name1 = job.Name;

            job.IsEnabled = false;

            var name2 = job.Name;

            // Assert
            Assert.That(name1, Is.EqualTo("my-job"));
            Assert.That(name2, Is.EqualTo("my-job"));
        }

        [Test]
        public void Name_JobIsRunningOrStopped_ReturnsValidName()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.IsEnabled = true;

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await Task.Delay(5000, token);
            };

            job.ForceStart();

            // Act
            var nameWhenRunning = job.Name;
            job.Cancel();
            var nameAfterStopped = job.Name;

            // Assert
            Assert.That(nameWhenRunning, Is.EqualTo("my-job"));
            Assert.That(nameAfterStopped, Is.EqualTo("my-job"));
        }

        [Test]
        public void Name_JobIsDisposed_ReturnsValidName()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            job.Dispose();

            // Act
            var name = job.Name;

            // Assert
            Assert.That(name, Is.EqualTo("my-job"));
        }

        #endregion

        #region IJob.IsEnabled

        [Test]
        public void IsEnabled_JustCreatedJob_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsEnabled_ChangedToTrue_ReturnsTrue()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            job.IsEnabled = true;

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.True);
        }

        [Test]
        public void IsEnabled_ChangedToTrueThenToFalse_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            job.IsEnabled = true;
            job.IsEnabled = false;

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsEnabled_WasTrueThenDisposed_ReturnsTrue()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            job.IsEnabled = true;
            job.Dispose();

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.True);
        }

        [Test]
        public void IsEnabled_WasFalseThenDisposed_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            job.Dispose();

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsEnabled_IsDisposed_ThrowsJobObjectDisposedException()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            job.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => job.IsEnabled = true);

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'my-job' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("my-job"));
        }

        #endregion

        #region IJob.Schedule

        [Test]
        public void Schedule_JustCreatedJob_ReturnsNeverSchedule()
        {
            // Arrange
            var now = "2020-09-15Z".ToUtcDayOffset();

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            var schedule = job.Schedule;
            var dueTime = schedule.GetDueTimeAfter(now);

            // Assert
            Assert.That(schedule, Is.Not.Null);
            Assert.That(schedule.GetType().FullName, Is.EqualTo("TauCode.Working.Schedules.NeverSchedule"));

            Assert.That(dueTime.Year, Is.EqualTo(9000)); // 'Never'

            jobManager.Dispose();
        }

        [Test]
        public void Schedule_SetNull_ThrowsArgumentNullException()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => job.Schedule = null);

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo(nameof(IJob.Schedule)));
        }

        [Test]
        public void Schedule_SetValidValueForEnabledOrDisabledJob_SetsValue()
        {
            // Arrange
            var now = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            ISchedule schedule1 = new SimpleSchedule(SimpleScheduleKind.Minute, 1, now);
            ISchedule schedule2 = new SimpleSchedule(SimpleScheduleKind.Day, 1, now);

            // Act
            job.Schedule = schedule1;
            var updatedSchedule1 = job.Schedule;
            var dueTime1 = job.GetInfo(null).NextDueTime;

            job.IsEnabled = true;

            job.Schedule = schedule2;
            var updatedSchedule2 = job.Schedule;
            var dueTime2 = job.GetInfo(null).NextDueTime;

            // Assert
            Assert.That(updatedSchedule1, Is.SameAs(schedule1));
            Assert.That(dueTime1, Is.EqualTo(now.AddMinutes(1)));

            Assert.That(updatedSchedule2, Is.SameAs(schedule2));
            Assert.That(dueTime2, Is.EqualTo(now.AddDays(1)));
        }

        [Test]
        public async Task Schedule_SetAndStarted_ReflectedInCurrentRunAndUpdatesToNextDueTime()
        {
            // Arrange
            var now = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await Task.Delay(10000, token); // long run
            };
            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            job.IsEnabled = true;

            // Act
            job.Schedule = schedule; // will fire at 00:01

            await Task.Delay(1010); // let job start

            // Assert
            var info = job.GetInfo(null);
            var currentRunNullable = info.CurrentRun;

            Assert.That(currentRunNullable, Is.Not.Null);
            var currentRun = currentRunNullable.Value;

            Assert.That(currentRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            Assert.That(currentRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            Assert.That(currentRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(TimeSpan.FromMilliseconds(20)));

            // due time is 00:02 after start
            Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(2)));
        }

        [Test]
        public async Task Schedule_SetAndStartedAndCompleted_ReflectedInOldRuns()
        {
            // Arrange

            var DEFECT = TimeSpan.FromMilliseconds(30);

            var now = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await Task.Delay(1500, token); // 1.5 second to complete
            };
            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            job.IsEnabled = true;

            // Act
            job.Schedule = schedule; // will fire at 00:01

            await Task.Delay(
                1000 + // 0th due time
                DEFECT.Milliseconds +
                1500 +
                DEFECT.Milliseconds); // let job start, finish, and wait more 30 ms.

            // Assert
            var info = job.GetInfo(null);
            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(3)));

            var pastRun = info.Runs.Single();

            Assert.That(pastRun.RunIndex, Is.EqualTo(0));
            Assert.That(pastRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            Assert.That(pastRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            Assert.That(pastRun.DueTimeWasOverridden, Is.False);

            Assert.That(pastRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(DEFECT));
            Assert.That(
                pastRun.EndTime,
                Is.EqualTo(pastRun.StartTime.AddSeconds(1.5)).Within(DEFECT));

            Assert.That(pastRun.Status, Is.EqualTo(JobRunStatus.Succeeded));
        }

        #endregion

        //====================================================================================

        [Test]
        [Ignore("todo")]
        public void GetInfo_NoArguments_ReturnsJobInfo()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
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
        [Ignore("todo")]
        public void ManualChangeDueTime_NotNull_DueTimeIsChanged()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager();
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
        [Ignore("todo")]
        public async Task ForceStart_NotStarted_RunsSuccessfully()
        {
            // Arrange
            var now = "2020-09-11Z".ToUtcDayOffset();
            TimeProvider.Override(now);

            using IJobManager jobManager = TestHelper.CreateJobManager();
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
        [Ignore("todo")]
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
            job.IsEnabled = true;

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


        // todo: IJob.Schedule
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
