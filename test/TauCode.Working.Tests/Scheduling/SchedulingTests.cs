using NUnit.Framework;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;
using TauCode.Working.Jobs.Schedules;


namespace TauCode.Working.Tests.Scheduling
{
    [TestFixture]
    public class SchedulingTests
    {
        [Test]
        public async Task Todo_Test()
        {
            // Arrange
            IJobManager scheduleManager = new JobManager();
            scheduleManager.Start();

            var now = TimeProvider.GetCurrent().TruncateMilliseconds();
            
            var sb = new StringBuilder();

            var schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);
            //var worker = new MyWorker(schedule);

            // Act
            scheduleManager.RegisterJob(
                "my-job", 
                () => Task.Run(() =>
                {
                    sb.AppendLine(TimeProvider.GetCurrent().ToString("O", CultureInfo.InvariantCulture));
                }),
                schedule);

            await Task.Delay(6 * 1000); // todo

            // Assert
            var res = sb.ToString();
            scheduleManager.Dispose();
        }
    }
}
