using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;
using TauCode.Working.Jobs.Schedules;


namespace TauCode.Working.Tests.Scheduling
{
    [TestFixture]
    public class JobTests
    {
        [Test]
        public async Task Todo_Test()
        {
            // Arrange
            IJobManager scheduleManager = new JobManager();
            scheduleManager.Start();

            var now = TimeProvider.GetCurrent().TruncateMilliseconds();

            //var sb = new StringBuilder();

            var schedule = new SimpleSchedule(
                SimpleScheduleKind.Hour,
                1,
                now,
                new List<TimeSpan>()
                {
                    TimeSpan.FromSeconds(2),
                });

            

            // Act
            scheduleManager.RegisterJob(
                "my-job",
                (writer, token) => Task.Run(async () =>
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            await writer.WriteLineAsync(TimeProvider.GetCurrent().ToString("O", CultureInfo.InvariantCulture));
                            //await Task.Delay(100, token);

                            var got = token.WaitHandle.WaitOne(100);
                            if (got)
                            {
                                await writer.WriteLineAsync("Got cancel!");
                                throw new TaskCanceledException();
                            }

                            //if (token.IsCancellationRequested)
                            //{
                            //    await writer.WriteLineAsync("Cancellation requested!");
                            //    return false;
                            //}
                        }

                        return true;
                    },
                    token),
                schedule);

            await Task.Delay(2001);
            scheduleManager.CancelRunningJob("my-job");
            
            await Task.Delay(60 * 60 * 1000); // todo


            // Assert
            //var res = sb.ToString();
            scheduleManager.Dispose();
        }

        //[Test]
        //public async Task RegisterJob_ValidJob_JobIsRegistered()
        //{
        //    // Arrange
        //    IJobManager scheduleManager = new JobManager();
        //    scheduleManager.Start();
        //    var schedule = new SimpleSchedule(
        //        SimpleScheduleKind.Day,
        //        1,
        //        new DateTime(2020, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc));

        //    TimeProvider.Override(new ConstTimeProvider(new DateTime(2020, 7, 5, 0, 0, 0, kind: DateTimeKind.Utc)));

        //    // Act
        //    scheduleManager.RegisterJob("job1", (writer, source) => null, schedule);

        //    // Assert
        //    scheduleManager.GetJobInfo("job1", true);
        //}
    }
}
