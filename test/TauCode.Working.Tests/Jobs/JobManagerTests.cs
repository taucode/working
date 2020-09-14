using NUnit.Framework;
using System;
using System.IO;
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
    public class JobManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            TimeProvider.Reset();
        }

        #region JobManager.ctor

        /// <summary>
        /// ===========
        /// Arrange:
        /// 
        /// ===========
        /// Act: 
        /// 1. Instance of <see cref="JobManager"/> is created.
        ///
        /// ===========
        /// Assert:
        /// 1. Instance is not started.
        /// 2. Instance is not disposed.
        /// 3. GetNames() returns 0 elements.
        /// </summary>
        [Test]
        public void Constructor_NoArguments_CreatesInstance()
        {
            // Arrange

            // Act
            IJobManager jobManager = new JobManager();

            // Assert
            Assert.That(jobManager.IsRunning, Is.False);
            Assert.That(jobManager.IsDisposed, Is.False);

            jobManager.Dispose();
        }

        #endregion

        #region IJobManager.Start

        [Test]
        public void Start_NotStarted_Starts()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            jobManager.Start();

            // Assert
            Assert.That(jobManager.IsRunning, Is.True);
            Assert.That(jobManager.IsDisposed, Is.False);
            Assert.That(jobManager.GetNames(), Has.Count.Zero);

            jobManager.Dispose();
        }

        [Test]
        public void Start_AlreadyStarted_ThrowsInvalidJobOperationException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Start());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is already running"));
            jobManager.Dispose();
        }

        [Test]
        public void Start_AlreadyDisposed_ThrowsException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => jobManager.Start());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo(typeof(IJobManager).FullName));

            jobManager.Dispose();
        }

        #endregion

        #region IJobManager.IsRunning

        [Test]
        public void IsRunning_NotStarted_ReturnsFalse()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            var isRunning = jobManager.IsRunning;

            // Assert
            Assert.That(isRunning, Is.False);
        }

        [Test]
        public void IsRunning_Started_ReturnsTrue()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var isRunning = jobManager.IsRunning;

            // Assert
            Assert.That(isRunning, Is.True);

            jobManager.Dispose(); // otherwise
        }

        [Test]
        public void IsRunning_NotStartedThenDisposed_ReturnsFalse()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            var isRunning = jobManager.IsRunning;

            // Assert
            Assert.That(isRunning, Is.False);
        }

        [Test]
        public void IsRunning_StartedThenDisposed_ReturnsFalse()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Dispose();

            // Act
            var isRunning = jobManager.IsRunning;

            // Assert
            Assert.That(isRunning, Is.False);
        }

        #endregion

        #region IJobManager.IsDisposed

        [Test]
        public void IsDisposed_NotStarted_ReturnsFalse()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            var isDisposed = jobManager.IsDisposed;

            // Assert
            Assert.That(isDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_Started_ReturnsFalse()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var isDisposed = jobManager.IsDisposed;

            // Assert
            Assert.That(isDisposed, Is.False);
            jobManager.Dispose();
        }

        [Test]
        public void IsDisposed_NotStartedThenDisposed_ReturnsTrue()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            var isDisposed = jobManager.IsDisposed;

            // Assert
            Assert.That(isDisposed, Is.True);
        }

        [Test]
        public void IsDisposed_StartedThenDisposed_ReturnsTrue()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Dispose();

            // Act
            var isDisposed = jobManager.IsDisposed;

            // Assert
            Assert.That(isDisposed, Is.True);
        }

        #endregion

        #region IJobManager.Create

        [Test]
        public void Create_NotStarted_ThrowsInvalidJobOperationException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Create("job1"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' not started."));
        }

        [Test]
        public void Create_Started_ReturnsJob()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var job = jobManager.Create("job1");

            // Assert
            Assert.That(job.Name, Is.EqualTo("job1"));

            var now = TimeProvider.GetCurrent();
            Assert.That(job.Schedule.GetDueTimeAfter(now), Is.EqualTo(JobExtensions.Never));
            Assert.That(job.Routine, Is.Not.Null);
            Assert.That(job.Parameter, Is.Null);
            Assert.That(job.ProgressTracker, Is.Null);
            Assert.That(job.Output, Is.Null);

        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Create_BadJobName_ThrowsArgumentException(string badJobName)
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => jobManager.Create(badJobName));

            // Assert
            Assert.That(ex.Message, Does.StartWith("Job name cannot be null or empty."));
            Assert.That(ex.ParamName, Is.EqualTo("jobName"));
        }

        [Test]
        public void Create_NameAlreadyExists_ThrowsInvalidJobOperationException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            var name = "job1";
            jobManager.Create(name);

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Create(name));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"Job '{name}' already exists."));
        }

        [Test]
        public void Create_Disposed_ThrowsJobObjectIsDisposedException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => jobManager.Create("job1"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo(typeof(IJobManager).FullName));

        }

        #endregion

        #region IJobManager.GetNames

        [Test]
        public void GetNames_NotStarted_ThrowsInvalidJobOperationException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.GetNames());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' not started."));
        }

        [Test]
        public void GetNames_Started_ReturnsJobNames()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Create("job1");
            jobManager.Create("job2");

            // Act
            var jobNames = jobManager.GetNames();

            // Assert
            CollectionAssert.AreEquivalent(new string[] { "job1", "job2" }, jobNames);
        }

        [Test]
        public void GetNames_Disposed_ThrowsJobObjectIsDisposedException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Create("job1");
            jobManager.Create("job2");
            jobManager.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => jobManager.GetNames());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo(typeof(IJobManager).FullName));
        }

        #endregion

        #region IJobManager.Get

        [Test]
        public void Get_NotStarted_ThrowsInvalidJobOperationException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Get("my-job"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' not started."));
        }

        [Test]
        public void Get_Started_ReturnsJob()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            var job1 = jobManager.Create("job1");
            var job2 = jobManager.Create("job2");

            // Act
            var gotJob1 = jobManager.Get("job1");

            // Assert
            Assert.That(gotJob1, Is.SameAs(job1));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Get_BadJobName_ThrowsArgumentException(string badJobName)
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => jobManager.Get(badJobName));

            // Assert
            Assert.That(ex.Message, Does.StartWith("Job name cannot be null or empty."));
            Assert.That(ex.ParamName, Is.EqualTo("jobName"));
        }

        [Test]
        public void Get_NonExistingJobName_ThrowsInvalidJobOperationException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Create("job1");
            jobManager.Create("job2");

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Get("non-existing"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Job not found: 'non-existing'."));
        }

        [Test]
        public void Get_Disposed_ThrowsJobObjectDisposedException()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Create("job1");
            jobManager.Create("job2");
            jobManager.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => jobManager.Get("job1"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo(typeof(IJobManager).FullName));
        }

        #endregion

        #region IJobManager.Dispose

        [Test]
        public void Dispose_NotStarted_Disposes()
        {
            // Arrange
            IJobManager jobManager = new JobManager();

            // Act
            jobManager.Dispose();

            // Assert
            Assert.That(jobManager.IsDisposed, Is.True);
        }

        [Test]
        [Ignore("todo")]
        public void Dispose_Started_Disposes()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            jobManager.Dispose();

            // Assert
            Assert.That(jobManager.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_AlreadyDisposed_RunsOk()
        {
            // Arrange
            IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            jobManager.Dispose();

            // Assert
            Assert.That(jobManager.IsDisposed, Is.True);
        }

        [Test]
        public async Task Dispose_JobsCreated_DisposesAndJobsAreCanceledAndDisposed()
        {
            // Arrange
            var fakeNow = "2020-01-01Z".ToUtcDayOffset();
            TimeProvider.Override(ShiftedTimeProvider.CreateTimeMachine(fakeNow));

            IJobManager jobManager = new JobManager();
            jobManager.Start();
            var job1 = jobManager.Create("job1");
            var job2 = jobManager.Create("job2");

            job1.Output = new StringWriterWithEncoding(Encoding.UTF8);
            job2.Output = new StringWriterWithEncoding(Encoding.UTF8);

            async Task Routine(object parameter, IProgressTracker tracker, TextWriter output, CancellationToken token)
            {
                for (var i = 0; i < 100; i++)
                {
                    var time = TimeProvider.GetCurrent();
                    await output.WriteLineAsync($"Iteration {i}: {time.Second:D2}:{time.Millisecond:D3}");

                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        time = TimeProvider.GetCurrent();
                        await output.WriteLineAsync($"Canceled! {time.Second:D2}:{time.Millisecond:D3}");
                        throw;
                    }
                }
            }

            ISchedule schedule = new SimpleSchedule(
                SimpleScheduleKind.Second,
                1,
                fakeNow.AddMilliseconds(400));

            //var ke = TimeProvider.GetCurrent();
            //var kk = 3;

            throw new NotImplementedException();
            //job1.UpdateSchedule(schedule);
            //job2.UpdateSchedule(schedule);

            //job1.Routine = Routine;
            //job2.Routine = Routine;

            //await Task.Delay(2500); // 2 iterations should be completed

            //// Act
            //var jobInfoBeforeDispose1 = job1.GetInfo(null);
            //var jobInfoBeforeDispose2 = job2.GetInfo(null);

            //jobManager.Dispose();

            //// Assert
            //throw new NotImplementedException();

            ////foreach (var job in new IJob[] {job1, job2})
            ////{
            ////    //Assert.That(ex.Message, Is.EqualTo("Job not found: 'non-existing'."));
            ////}
        }

        #endregion
    }
}
