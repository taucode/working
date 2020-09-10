using NUnit.Framework;

// todo clean up
namespace TauCode.Working.Tests.Scheduling
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

        [Test]
        public void Todo1()
        {
            // Arrange

            // Act

            // Assert
        }
        
    }
}
