using NUnit.Framework;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;

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

            Assert.That(info.IsEnabled, Is.True);
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

    }
}
