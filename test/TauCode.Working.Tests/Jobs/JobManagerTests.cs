using NUnit.Framework;
using System;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;

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
            using IJobManager jobManager = new JobManager();

            // Act
            jobManager.Start();

            // Assert
            Assert.That(jobManager.IsRunning, Is.True);
            Assert.That(jobManager.IsDisposed, Is.False);
            Assert.That(jobManager.GetNames(), Has.Count.Zero);
        }

        [Test]
        public void Start_AlreadyStarted_ThrowsInvalidJobOperationException()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Start());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is already running"));
        }

        [Test]
        public void Start_AlreadyDisposed_ThrowsException()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => jobManager.Start());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo(typeof(IJobManager).FullName));
        }

        #endregion

        #region IJobManager.IsRunning

        [Test]
        public void IsRunning_NotStarted_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();

            // Act
            var isRunning = jobManager.IsRunning;

            // Assert
            Assert.That(isRunning, Is.False);
        }

        [Test]
        public void IsRunning_Started_ReturnsTrue()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var isRunning = jobManager.IsRunning;

            // Assert
            Assert.That(isRunning, Is.True);
        }

        [Test]
        public void IsRunning_NotStartedThenDisposed_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();

            // Act
            var isDisposed = jobManager.IsDisposed;

            // Assert
            Assert.That(isDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_Started_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
            jobManager.Start();

            // Act
            var isDisposed = jobManager.IsDisposed;

            // Assert
            Assert.That(isDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_NotStartedThenDisposed_ReturnsTrue()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Create("job1"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' not started."));
        }

        [Test]
        public void Create_Started_ReturnsJob()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.GetNames());

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' not started."));
        }

        [Test]
        public void GetNames_Started_ReturnsJobNames()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
            jobManager.Start();
            jobManager.Create("job1");
            jobManager.Create("job2");

            // Act
            var jobNames = jobManager.GetNames();

            // Assert
            CollectionAssert.AreEquivalent(new string[] {"job1", "job2"}, jobNames);
        }

        [Test]
        public void GetNames_Disposed_ThrowsJobObjectIsDisposedException()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();

            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Get("my-job"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"'{typeof(IJobManager).FullName}' not started."));
        }

        [Test]
        public void Get_Started_ReturnsJob()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();

            // Act
            jobManager.Dispose();

            // Assert
            Assert.That(jobManager.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_Started_Disposes()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
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
            using IJobManager jobManager = new JobManager();
            jobManager.Dispose();

            // Act
            jobManager.Dispose();

            // Assert
            Assert.That(jobManager.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_JobsCreated_DisposesAndJobsAreCanceledAndDisposed()
        {
            // Arrange
            using IJobManager jobManager = new JobManager();
            jobManager.Start();
            var job1 = jobManager.Create("job1");
            var job2 = jobManager.Create("job2");



            // Act
            var ex = Assert.Throws<InvalidJobOperationException>(() => jobManager.Get("non-existing"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Job not found: 'non-existing'."));
        }

        #endregion
    }
}
