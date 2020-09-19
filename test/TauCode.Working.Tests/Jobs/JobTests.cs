using Moq;
using NUnit.Framework;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;
using TauCode.Working.Schedules;

// todo clean up
namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public class JobTests
    {
        private StringWriterWithEncoding _logWriter;

        [SetUp]
        public void SetUp()
        {
            TimeProvider.Reset();

            _logWriter = new StringWriterWithEncoding(Encoding.UTF8);
            Log.Logger = new LoggerConfiguration()
                //.Filter.ByIncludingOnly(x => x.Properties.ContainsKey("taucode.working"))
                .MinimumLevel.Debug()
                .WriteTo.TextWriter(_logWriter)
                .CreateLogger();
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
        public async Task Schedule_SetValidValue_SetsSchedule()
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

            await Task.Delay(1030); // let job start

            // Assert
            var info = job.GetInfo(null);
            var currentRunNullable = info.CurrentRun;

            Assert.That(currentRunNullable, Is.Not.Null);  // todo: was null once :(
            var currentRun = currentRunNullable.Value;

            Assert.That(currentRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            Assert.That(currentRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            Assert.That(currentRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(TimeSpan.FromMilliseconds(20)));

            // due time is 00:02 after start
            Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(2)));
        }

        // todo: causes failures sometimes.
        // do not remove this todo until bug is found and fixed.
        // Expected: 2000-01-01 00:00:03+00:00
        // But was:  2000-01-01 00:00:01+00:00 
        // Looks like Vice never was started.
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
            // todo
            //Expected: 2000 - 01 - 01 00:00:03 + 00:00
            //But was:  2000 - 01 - 01 00:00:01 + 00:00

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

            Assert.Pass(_logWriter.ToString());
        }

        /// <summary>
        /// 0---------1---------2---------3---------
        ///           |_____1.5s_____|               (was the plan)
        /// ______1.4s____X_________________________ (cancel)
        /// </summary>
        [Test]
        public async Task Schedule_SetAndStartedAndCanceled_ReflectedInOldRuns()
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

            await Task.Delay(1400 + DEFECT.Milliseconds);
            var canceled = job.Cancel(); // will be canceled almost right after start

            Assert.That(canceled, Is.True);
            await Task.Delay(DEFECT); // let context finalize

            // Assert
            var info = job.GetInfo(null);
            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(2)));
            Assert.That(info.NextDueTimeIsOverridden, Is.False);

            var pastRun = info.Runs.Single();

            Assert.That(pastRun.RunIndex, Is.EqualTo(0));
            Assert.That(pastRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            Assert.That(pastRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            Assert.That(pastRun.DueTimeWasOverridden, Is.False);

            Assert.That(pastRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(DEFECT));
            Assert.That(
                pastRun.EndTime,
                Is.EqualTo(pastRun.StartTime.AddSeconds(0.4)).Within(DEFECT * 2));

            Assert.That(pastRun.Status, Is.EqualTo(JobRunStatus.Canceled));
        }

        /// <summary>
        /// 0---------1---------2---------3---------
        ///           |_____1.5s_____!              (exception)
        /// </summary>
        [Test]
        public async Task Schedule_SetAndStartedAndFaulted_ReflectedInOldRuns()
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
                await Task.Delay(1500, token); // 1.5 second runs with no problem...
                throw new ApplicationException("BAD_NEWS"); // ...and then throws!
            };
            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            job.IsEnabled = true;

            // Act
            job.Schedule = schedule; // will fire at 00:01

            await Task.Delay(2800); // by this time, job will end due to the exception "BAD_NEWS" and finalize.

            // Assert
            var info = job.GetInfo(null);
            Assert.That(info.CurrentRun, Is.Null);



            Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(3)));
            // todo
            //Expected: 2000 - 01 - 01 00:00:03 + 00:00
            //But was:  2000 - 01 - 01 00:00:01 + 00:00



            Assert.That(info.NextDueTimeIsOverridden, Is.False);

            var pastRun = info.Runs.Single();

            Assert.That(pastRun.RunIndex, Is.EqualTo(0));
            Assert.That(pastRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            Assert.That(pastRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            Assert.That(pastRun.DueTimeWasOverridden, Is.False);

            Assert.That(pastRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(DEFECT));
            Assert.That(
                pastRun.EndTime,
                Is.EqualTo(pastRun.StartTime.AddSeconds(1.5)).Within(DEFECT * 2));

            Assert.That(pastRun.Status, Is.EqualTo(JobRunStatus.Faulted));
            Assert.That(pastRun.Exception, Is.TypeOf<ApplicationException>());
            Assert.That(pastRun.Exception, Has.Message.EqualTo("BAD_NEWS"));
        }

        [Test]
        public async Task Schedule_DueTimeWasOverriddenThenScheduleIsSet_OverriddenDueTimeIsDiscardedAndScheduleIsSet()
        {
            // Arrange

            var DEFECT = TimeSpan.FromMilliseconds(30);

            var now = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.OverrideDueTime(now.AddSeconds(2.5));

            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            // Act
            await Task.Delay(1800);
            job.Schedule = schedule;

            // Assert
            var info = job.GetInfo(null);
            Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(2)));
            Assert.That(info.NextDueTimeIsOverridden, Is.False);
        }

        /// <summary>
        /// 0---------1-*---*---2-*-------3-*-------4   * - routine checks due time
        ///           |____________1.5s_____________|
        /// ...............!..^.........^.........^..     ! - schedule2 is set; ^ - 'schedule2' due time
        /// </summary>
        [Test]
        public async Task Schedule_SetDuringRun_DoesNotAffectRunButAppliesToDueTime()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");
            var schedule1 = new SimpleSchedule(SimpleScheduleKind.Second, 1, start);
            job.Schedule = schedule1; // job will be started by due time of this schedule

            DateTimeOffset dueTime1 = default;
            DateTimeOffset dueTime2 = default;
            DateTimeOffset dueTime3 = default;
            DateTimeOffset dueTime4 = default;

            job.Routine = async (parameter, tracker, output, token) =>
            {
                Log.Debug("Entered routine.");

                // start + 1.2s: due time is (start + 2s), set by schedule1
                await timeMachine.WaitUntilSecondsElapse(start, 1.2, token);
                dueTime1 = job.GetInfo(0).NextDueTime; // should be 2s

                await timeMachine.WaitUntilSecondsElapse(start, 1.7, token);
                dueTime2 = job.GetInfo(0).NextDueTime;

                await timeMachine.WaitUntilSecondsElapse(start, 2.2, token);
                dueTime3 = job.GetInfo(0).NextDueTime;

                await timeMachine.WaitUntilSecondsElapse(start, 3.2, token);
                dueTime4 = job.GetInfo(0).NextDueTime;

                await Task.Delay(TimeSpan.FromHours(2), token);

                Log.Debug("Exited routine.");
            };

            job.IsEnabled = true;

            // Act
            var schedule2 = new SimpleSchedule(SimpleScheduleKind.Second, 1, start.AddSeconds(1.8));
            await timeMachine.WaitUntilSecondsElapse(start, 1.4);
            job.Schedule = schedule2;

            await timeMachine.WaitUntilSecondsElapse(start, 4);

            // Assert
            Assert.That(dueTime1, Is.EqualTo(start.AddSeconds(2)));
            Assert.That(dueTime2, Is.EqualTo(start.AddSeconds(1.8)));
            Assert.That(dueTime3, Is.EqualTo(start.AddSeconds(2.8)));
            Assert.That(dueTime4, Is.EqualTo(start.AddSeconds(3.8)));

            Assert.Pass(_logWriter.ToString());
        }

        [Test]
        public void Schedule_SetThenDisposed_EqualsToLastOne()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var schedule1 = new SimpleSchedule(SimpleScheduleKind.Second, 1, start);
            var schedule2 = new SimpleSchedule(SimpleScheduleKind.Minute, 1, start);
            var schedule3 = new SimpleSchedule(SimpleScheduleKind.Hour, 1, start);
            

            job.Schedule = schedule1;
            var readSchedule1 = job.Schedule;

            job.Schedule = schedule2;
            var readSchedule2 = job.Schedule;

            // Act
            job.Schedule = schedule3;
            job.Dispose();
            var readSchedule3 = job.Schedule;

            // Assert
            Assert.That(readSchedule1, Is.SameAs(schedule1));
            Assert.That(readSchedule2, Is.SameAs(schedule2));
            Assert.That(readSchedule3, Is.SameAs(schedule3));
        }

        [Test]
        public void Schedule_DisposedThenSet_ThrowsJobObjectDisposedException()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            job.Dispose();
            var ex = Assert.Throws<JobObjectDisposedException>(() =>
                job.Schedule = new SimpleSchedule(SimpleScheduleKind.Minute, 1, start.AddHours(1)));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'my-job' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("my-job"));
        }

        [Test]
        public async Task Schedule_ScheduleThrows_DueTimeSetToNever()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var n = 0;

            job.IsEnabled = true;
            job.Routine = (parameter, tracker, output, token) =>
            {
                n = 100;
                return Task.CompletedTask;
            };

            // set some normal schedule first
            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Minute, 1, start);
            var normalDueTime = job.GetInfo(null).NextDueTime;

            var exception = new NotSupportedException("I do not support this!");

            // Act
            var scheduleMock = new Mock<ISchedule>();
            scheduleMock
                .Setup(x => x.GetDueTimeAfter(It.IsAny<DateTimeOffset>()))
                .Throws(exception);

            job.Schedule = scheduleMock.Object;

            await Task.Delay(3000); // let job try to start (it should not start though)

            var info = job.GetInfo(null);
            var faultedDueTime = info.NextDueTime;

            // Assert
            Assert.That(normalDueTime, Is.EqualTo(start.AddMinutes(1)));
            Assert.That(faultedDueTime.Year, Is.EqualTo(9000));

            // job never ran
            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.Runs, Is.Empty);
            Assert.That(info.RunCount, Is.Zero);
            Assert.That(n, Is.EqualTo(0));

            var log = _logWriter.ToString();
            Assert.That(log, Does.Contain($"{exception.GetType().FullName}: {exception.Message}"));

            Assert.Pass(_logWriter.ToString());
        }

        [Test]
        public async Task Schedule_ScheduleReturnsDueTimeBeforeNow_DueTimeSetToNever()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var n = 0;

            job.IsEnabled = true;
            job.Routine = (parameter, tracker, output, token) =>
            {
                n = 100;
                return Task.CompletedTask;
            };

            // set some normal schedule first
            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Minute, 1, start);
            var normalDueTime = job.GetInfo(null).NextDueTime;

            // Act
            var scheduleMock = new Mock<ISchedule>();
            scheduleMock
                .Setup(x => x.GetDueTimeAfter(It.IsAny<DateTimeOffset>()))
                .Returns(start.AddSeconds(-1)); // due time in the past

            job.Schedule = scheduleMock.Object;

            await Task.Delay(3000); // let job try to start (it should not start though)

            var info = job.GetInfo(null);
            var faultedDueTime = info.NextDueTime;

            // Assert
            Assert.That(normalDueTime, Is.EqualTo(start.AddMinutes(1)));
            Assert.That(faultedDueTime.Year, Is.EqualTo(9000));

            // job never ran
            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.Runs, Is.Empty);
            Assert.That(info.RunCount, Is.Zero);
            Assert.That(n, Is.EqualTo(0));

            var log = _logWriter.ToString();
            Assert.That(log, Does.Contain($"Due time is earlier than current time. Due time changed to 'never'."));

            Assert.Pass(_logWriter.ToString());
        }

        [Test]
        public async Task Schedule_ScheduleReturnsDueTimeAfterNever_DueTimeSetToNever()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var n = 0;

            job.IsEnabled = true;
            job.Routine = (parameter, tracker, output, token) =>
            {
                n = 100;
                return Task.CompletedTask;
            };

            // set some normal schedule first
            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Minute, 1, start);
            var normalDueTime = job.GetInfo(null).NextDueTime;

            // Act
            var scheduleMock = new Mock<ISchedule>();
            scheduleMock
                .Setup(x => x.GetDueTimeAfter(It.IsAny<DateTimeOffset>()))
                .Returns(DateTimeOffset.MaxValue); // due time in the past

            job.Schedule = scheduleMock.Object;

            await Task.Delay(3000); // let job try to start (it should not start though)

            var info = job.GetInfo(null);
            var faultedDueTime = info.NextDueTime;

            // Assert
            Assert.That(normalDueTime, Is.EqualTo(start.AddMinutes(1)));
            Assert.That(faultedDueTime.Year, Is.EqualTo(9000));

            // job never ran
            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.Runs, Is.Empty);
            Assert.That(info.RunCount, Is.Zero);
            Assert.That(n, Is.EqualTo(0));

            var log = _logWriter.ToString();
            Assert.That(log, Does.Contain("Due time is later than 'never'. Due time changed to 'never'."));

            Assert.Pass(_logWriter.ToString());
        }

        #endregion

        #region IJob.Routine

        // todo: IJob.Routine
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        [Test]
        public async Task Routine_JustCreatedJob_NotNullAndRunsSuccessfully()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, start.AddSeconds(2));
            job.IsEnabled = true;

            // Act
            var routine = job.Routine;
            await timeMachine.WaitUntilSecondsElapse(start, 2.7);
            jobManager.Dispose();

            var info = job.GetInfo(null);

            // Assert
            Assert.That(routine, Is.Not.Null);
            var run = info.Runs.First();
            Assert.That(run.Status, Is.EqualTo(JobRunStatus.Succeeded));
        }

        [Test]
        public void Routine_SetNull_ThrowsArgumentNullException()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => job.Routine = null);

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("Routine"));
        }

        /// <summary>
        /// 0---------1---------2---------3---------4---------5---------
        ///           |_R1:_1.5s_____|    |_R2:_1.5s_____|               (R1 - routine1, R2 - routine2)
        /// ______1.4s____!1_______________________________!2___________ (!1 - set routine2, !2 - dispose)
        /// </summary>
        [Test]
        public async Task Routine_SetOnTheFly_CompletesWithOldRoutineAndThenStartsWithNewRoutine()
        {
            Task task = default;

            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var output = new StringWriterWithEncoding(Encoding.UTF8);
            job.Output = output;

            job.Schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, start);

            async Task Routine1(object parameter, IProgressTracker tracker, TextWriter writer, CancellationToken token)
            {
                await writer.WriteAsync("First Routine!");
                await Task.Delay(1500, token);
            }

            async Task Routine2(object parameter, IProgressTracker tracker, TextWriter writer, CancellationToken token)
            {
                await writer.WriteAsync("Second Routine!");
                await Task.Delay(1500, token);
            }

            job.IsEnabled = true;
            job.Routine = Routine1;

            // Act
            await timeMachine.WaitUntilSecondsElapse(start, 1.4);
            job.Routine = Routine2;

            await timeMachine.WaitUntilSecondsElapse(start, 2.8);
            var output1 = output.ToString();

            await timeMachine.WaitUntilSecondsElapse(start, 4.8);
            var output2 = output.ToString();

            jobManager.Dispose();

            var info = job.GetInfo(null);

            // Assert
            Assert.That(info.NextDueTime, Is.EqualTo(start.AddSeconds(5)));

            Assert.That(info.CurrentRun, Is.Null);
            Assert.That(info.RunCount, Is.EqualTo(2));
            Assert.That(info.Runs, Has.Count.EqualTo(2));

            var run0 = info.Runs[0];
            Assert.That(run0.DueTime, Is.EqualTo(start.AddSeconds(1)));
            Assert.That(run0.Output, Is.EqualTo("First Routine!"));
            Assert.That(output1, Is.EqualTo("First Routine!"));

            var run1 = info.Runs[1];
            Assert.That(run1.DueTime, Is.EqualTo(start.AddSeconds(3)));
            Assert.That(run1.Output, Is.EqualTo("Second Routine!"));
            Assert.That(output2, Is.EqualTo("First Routine!Second Routine!"));
        }
        
        [Test]
        public void Routine_SetValidValueForEnabledOrDisabledJob_SetsValue()
        {
            // Arrange
            var now = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            JobDelegate routine1 = async (parameter, tracker, output, token) =>
            {
                await output.WriteAsync("First Routine!");
                await Task.Delay(1500, token);
            };

            JobDelegate routine2 = async (parameter, tracker, output, token) =>
            {
                await output.WriteAsync("Second Routine!");
                await Task.Delay(1500, token);
            };


            // Act
            job.Routine = routine1;
            var updatedRoutine1 = job.Routine;

            job.IsEnabled = true;

            job.Routine = routine2;
            var updatedRoutine2 = job.Routine;

            // Assert
            Assert.That(updatedRoutine1, Is.SameAs(routine1));

            Assert.That(updatedRoutine2, Is.SameAs(routine2));
        }

        /// <summary>
        /// 0---------1---------2---------3---------4---------5---------6
        ///           |_R1:_1.5s_____|              |_R2:_1.5s_____|      (R1 - routine1, R2 - routine2)
        /// ______________________________!______________________________ (! - set routine2)
        /// </summary>
        [Test]
        public async Task Routine_SetAfterPreviousRunCompleted_SetsValueAndRunsWithIt()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var output = new StringWriterWithEncoding(Encoding.UTF8);
            job.Output = output;

            JobDelegate routine1 = async (parameter, tracker, writer, token) =>
            {
                await writer.WriteAsync("First Routine!");
                await Task.Delay(1500, token);
            };

            JobDelegate routine2 = async (parameter, tracker, writer, token) =>
            {
                await writer.WriteAsync("Second Routine!");
                await Task.Delay(1500, token);
            };

            job.Schedule = new ConcreteSchedule(
                start.AddSeconds(1),
                start.AddSeconds(4));

            job.IsEnabled = true;

            // Act
            job.Routine = routine1;
            var updatedRoutine1 = job.Routine;

            await timeMachine.WaitUntilSecondsElapse(start, 2.9); // job with routine1 will complete
            var output1 = output.ToString();

            await timeMachine.WaitUntilSecondsElapse(start, 3.0);
            job.Routine = routine2;
            var updatedRoutine2 = job.Routine;

            await timeMachine.WaitUntilSecondsElapse(start, 6.0);
            var output2 = output.ToString();

            // Assert
            Assert.That(updatedRoutine1, Is.SameAs(routine1));
            Assert.That(output1, Is.EqualTo("First Routine!"));

            Assert.That(updatedRoutine2, Is.SameAs(routine2));
            Assert.That(output2, Is.EqualTo("First Routine!Second Routine!"));
        }

        [Test]
        public async Task Routine_Throws_LogsFaultedTask()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            var output = new StringWriterWithEncoding(Encoding.UTF8);
            job.Output = output;

            var exception = new NotSupportedException("Bye baby!");

            JobDelegate routine = (parameter, tracker, writer, token) =>
            {
                writer.WriteLine("Hi there!");
                throw exception;
            };

            job.Schedule = new ConcreteSchedule(
                start.AddSeconds(1));

            job.IsEnabled = true;

            // Act
            job.Routine = routine;
            var updatedRoutine = job.Routine;

            await timeMachine.WaitUntilSecondsElapse(start, 1.5); // will fail by this time
            var outputResult = output.ToString();

            var info = job.GetInfo(null);

            // Assert
            Assert.That(updatedRoutine, Is.SameAs(routine));
            Assert.That(outputResult, Does.Contain("Hi there!"));
            Assert.That(outputResult, Does.Contain(exception.ToString()));

            Assert.That(info.CurrentRun, Is.Null);

            Assert.That(info.RunCount, Is.EqualTo(1));
            var run = info.Runs.Single();

            Assert.That(run.Status, Is.EqualTo(JobRunStatus.Faulted));
            Assert.That(run.Exception, Is.SameAs(exception));
            Assert.That(run.Output, Does.Contain(exception.ToString()));
            
            var log = _logWriter.ToString();

            Assert.That(log, Does.Contain("Routine has thrown an exception."));

            Assert.Pass(log);
        }

        [Test]
        public async Task Routine_ReturnsCanceledTask_LogsCanceledTask()
        {
            // Arrange
            var start = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(start);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            using var source = new CancellationTokenSource();
            source.Cancel();

            jobManager.Start();
            var job = jobManager.Create("my-job");

            var output = new StringWriterWithEncoding(Encoding.UTF8);
            job.Output = output;

            JobDelegate routine = (parameter, tracker, writer, token) =>
            {
                writer.WriteLine("Hi there!");
                return Task.FromCanceled(source.Token);
            };

            job.Schedule = new ConcreteSchedule(
                start.AddSeconds(1));

            job.IsEnabled = true;

            // Act
            job.Routine = routine;
            var updatedRoutine = job.Routine;

            await timeMachine.WaitUntilSecondsElapse(start, 1.5); // will be canceled by this time
            var outputResult = output.ToString();

            var info = job.GetInfo(null);

            // Assert
            Assert.That(updatedRoutine, Is.SameAs(routine));
            Assert.That(outputResult, Does.Contain("Hi there!"));

            Assert.That(info.CurrentRun, Is.Null);

            Assert.That(info.RunCount, Is.EqualTo(1));
            var run = info.Runs.Single();

            Assert.That(run.Status, Is.EqualTo(JobRunStatus.Canceled));
            Assert.That(run.Exception, Is.Null);

            var log = _logWriter.ToString();

            Assert.That(log, Does.Contain($"Job 'my-job' completed synchronously. Reason of start was 'ScheduleDueTime'."));

            Assert.Pass(log);
        }


        // todo: if faulted => ...

        // todo: if ranToCompletion => ...



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
            Assert.That(info.NextDueTime, Is.EqualTo(manualDueTime));
            Assert.That(info.NextDueTimeIsOverridden, Is.True);
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

        //====================================================================================





        // todo: IJob.Parameter
        // - initially, null
        // - can be set to any value including null (if not running), be it enabled or disabled.
        // - if set while running, finishes job with current parameter; next run is performed with new parameter.
        // - when set after run completed, afterwards runs with new parameter value.
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        // todo: IJob.ProgressTracker
        // - initially, null
        // - can be set to any value including null (if not running), be it enabled or disabled.
        // - if set while running, finishes job with current progress tracker; next run is performed with new progress tracker.
        // - when set after run completed, afterwards runs with new progress tracker.
        // - after disposed, can be read.
        // - after disposed, cannot be set, throws.

        // todo: IJob.Output
        // - initially, null
        // - can be set to any value including null (if not running), be it enabled or disabled.
        // - if set while running, finishes job with current output; next run is performed with new output.
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
