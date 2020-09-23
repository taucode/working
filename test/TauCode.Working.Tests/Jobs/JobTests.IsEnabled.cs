using NUnit.Framework;
using System;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;

namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public partial class JobTests
    {
        [Test]
        public void IsEnabled_JustCreatedJob_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);
            var job = jobManager.Create("my-job");

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsEnabled_SetToFalseDuringRun_RunCompletesThenDoesNotStart()
        {
            // Arrange
            throw new NotImplementedException();
            //using IJobManager jobManager = TestHelper.CreateJobManager(true);
            //var job = jobManager.Create("my-job");
            //job.IsEnabled = true;

            //// Act
            //var isEnabled = job.IsEnabled;

            //// Assert
            //Assert.That(isEnabled, Is.True);
        }

        [Test]
        public void IsEnabled_ChangedToTrue_ReturnsTrue()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);
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
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

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
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var job = jobManager.Create("my-job");
            job.IsEnabled = true;
            job.Dispose();

            // Act
            var isEnabled = job.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.True);
        }

        [Test]
        public void IsEnabled_WasTrueThenFalse_JobDoesNotRunAnymore()
        {
            // Arrange

            // Act

            // Assert
            throw new NotImplementedException();
        }

        [Test]
        public void IsEnabled_WasFalseThenDisposed_ReturnsFalse()
        {
            // Arrange
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

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
            using IJobManager jobManager = TestHelper.CreateJobManager(true);

            var job = jobManager.Create("my-job");
            job.Dispose();

            // Act
            var ex = Assert.Throws<JobObjectDisposedException>(() => job.IsEnabled = true);

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'my-job' is disposed."));
            Assert.That(ex.ObjectName, Is.EqualTo("my-job"));
        }
    }
}
