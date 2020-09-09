using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;
using TauCode.Working.Jobs.Schedules;

namespace TauCode.Working.TestDemo.Lab
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var random = new Random();
            for (int i = 0; i < 400; i++)
            {
                await Task.Run(async () => await Task.Delay(12 + random.Next(18)));
            }

            var dummy = new SimpleSchedule(
                SimpleScheduleKind.Hour,
                4,
                TimeProvider.GetCurrent());

            await Task.Delay(34);

            IJobManager dummyMgr = new JobManager();
            dummyMgr.Start();

            await Task.Delay(11);
            dummyMgr.Dispose();


            // Arrange
            IJobManager scheduleManager = new JobManager();
            scheduleManager.Start();

            var now = TimeProvider.GetCurrent();

            Console.WriteLine($"*** NOW ***: {now.FormatTime()}");

            //var sb = new StringBuilder();

            var timeoutBeforeShowtime = TimeSpan.FromSeconds(2);
            var routineTimeout = TimeSpan.FromMilliseconds(100);

            var expectedRoutineStartTime = now + timeoutBeforeShowtime;

            var schedule = new SimpleSchedule(
                SimpleScheduleKind.Hour,
                1,
                now,
                new List<TimeSpan>()
                {
                    timeoutBeforeShowtime,
                });

            // Act
            scheduleManager.RegisterJob(
                "my-job",
                (writer, token) => Task.Run(async () =>
                    {
                        var routineStartedTime = TimeProvider.GetCurrent();

                        Console.WriteLine($"*** ROUTINE STARTED ***: {routineStartedTime.FormatTime()}");
                        Console.WriteLine($"*** DEFECT ***: {(expectedRoutineStartTime - routineStartedTime).TotalMilliseconds}");

                        for (var i = 0; i < 100; i++)
                        {
                            Console.WriteLine($"{i + 1}    :    {TimeProvider.GetCurrent().FormatTime()}");
                            await Task.Delay(routineTimeout, token);
                        }
                    },
                    token),
                schedule);


            var now2 = TimeProvider.GetCurrent();
            var adjustment = now2 - now;

            Console.WriteLine($"--- ADJUSTMENT ---:  {adjustment.TotalMilliseconds}");

            var ticksToAllow = 11;

            Console.WriteLine($"*** AWAITING STARTED ***: {TimeProvider.GetCurrent().FormatTime()}");
            await Task.Delay(timeoutBeforeShowtime - adjustment + ticksToAllow * routineTimeout);

            scheduleManager.CancelRunningJob("my-job");

            //await Task.Delay(60 * 60 * 1000); // todo


            // Assert
            //var res = sb.ToString();
            scheduleManager.Dispose();
        }
    }

    internal static class TestHelper
    {
        //internal static DateTime Trun-cateMilliseconds(this DateTime dateTime)
        //{
        //    return new DateTime(
        //        dateTime.Year,
        //        dateTime.Month,
        //        dateTime.Day,
        //        dateTime.Hour,
        //        dateTime.Minute,
        //        dateTime.Second,
        //        0,
        //        dateTime.Kind);
        //}

    }

}
