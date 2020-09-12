using NUnit.Framework;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;

// todo clean up
namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public class JobManagerTests
    {
        //[Test]
        //public async Task Constructor_NoArguments_RunsSimpleHappyPath()
        //{
        //    // Arrange
        //    IJobManager scheduleManager = new JobManager();
        //    scheduleManager.Start();

        //    var now = TimeProvider.GetCurrent();

        //    var schedule = new SimpleSchedule(
        //        SimpleScheduleKind.Hour,
        //        1,
        //        now,
        //        new List<TimeSpan>()
        //        {
        //            TimeSpan.FromSeconds(2),
        //        });



        //    // Act
        //    scheduleManager.Register(
        //        "my-job",
        //        (parameter, writer, token) => Task.Run(async () =>
        //            {
        //                for (int i = 0; i < 10; i++)
        //                {
        //                    await writer.WriteLineAsync(TimeProvider.GetCurrent().ToString("O", CultureInfo.InvariantCulture));

        //                    var got = token.WaitHandle.WaitOne(100);
        //                    if (got)
        //                    {
        //                        await writer.WriteLineAsync("Got cancel!");
        //                        throw new TaskCanceledException();
        //                    }
        //                }

        //                return true;
        //            },
        //            token),
        //        schedule,
        //        10);

        //    await Task.Delay(2001);
        //    scheduleManager.Cancel("my-job");

        //    // Assert
        //    scheduleManager.Dispose();
        //}

        [SetUp]
        public void SetUp()
        {
            TimeProvider.Reset();
        }

        [Test]
        public void Constructor_NoArguments_CreatesInstance()
        {
            // Arrange

            // Act
            IJobManager jobManager = new JobManager();

            // Assert
        }

        [Test]
        public void Create_ValidJobName_CreatesJob()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start(); // todo: ut cannot be started twice.

            // Act
            var job = jobManager.Create("my-job");

            // Assert
            var now = TimeProvider.GetCurrent();
            Assert.That(job.Schedule.GetDueTimeAfter(now), Is.EqualTo(JobExtensions.Never));
            Assert.That(job.Routine, Is.Not.Null);
            Assert.That(job.Parameter, Is.Null);
            Assert.That(job.ProgressTracker, Is.Null);
            Assert.That(job.Output, Is.Null);
        }

        [Test]
        public void GetJobNames_NoArguments_ReturnsJobNames()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start(); // todo: ut cannot be started twice.
            jobManager.Create("job1");
            jobManager.Create("job2");

            // Act
            var names = jobManager.GetNames();

            // Assert
            CollectionAssert.AreEquivalent(new[] { "job1", "job2" }, names);
        }

        [Test]
        public void Get_ValidName_ReturnsJob()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start(); // todo: ut cannot be started twice.
            var job = jobManager.Create("job1");

            // Act
            var gotJob = jobManager.Get("job1");

            // Assert
            Assert.That(job, Is.SameAs(gotJob));
        }

        // todo: IJobManager.Start
        // - cannot start twice
        // - cannot start disposed object

        // todo: IJobManager.IsRunning
        // - false if not started
        // - true if started
        // - false if disposed

        // todo: IJobManager.IsDisposed
        // - false if not started
        // - false if started
        // - true if disposed

        // todo: IJobManager.Create
        // - happy path
        // - exception if not started
        // - exception on bad job name
        // - exception on dup
        // - exception if disposed

        // todo: IJobManager.GetJobNames
        // - happy path
        // - exception if not started
        // - exception if disposed

        // todo: IJobManager.Get
        // - happy path
        // - exception if not started
        // - exception on bad job name
        // - exception if disposed

        // todo: IJobManager.Dispose
        // - happy path on started (serilog)
        // - happy path on not started (serilog)
        // - exception if called twice

    }
}
