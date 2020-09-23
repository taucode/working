using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;
using TauCode.Working.Schedules;

namespace TauCode.Labor.TestDemo.Lab
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Filter.ByIncludingOnly(x => x.Properties.ContainsKey("taucode.working"))
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var cnt = 40;
            for (int i = 0; i < cnt; i++)
            {
                Console.WriteLine(i);
                await FailingTest();
            }
        }

        private static async Task FailingTest()
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

            //job.Output = new StringWriterWithEncoding(Encoding.UTF8);

            job.Output = Console.Out;

            var idx = 0;

            job.Routine = async (parameter, tracker, output, token) =>
            {
                idx++;
                await output.WriteLineAsync("Entered routinush.");

                // start + 1.2s: due time is (start + 2s), set by schedule1
                await timeMachine.WaitUntilSecondsElapse(start, 1.2, token);
                dueTime1 = job.GetInfo(0).NextDueTime; // should be 2s

                await timeMachine.WaitUntilSecondsElapse(start, 1.7, token);
                dueTime2 = job.GetInfo(0).NextDueTime;

                await Task.Delay(TimeSpan.FromHours(2), token);
            };

            job.IsEnabled = true;

            // Act
            var schedule2 = new SimpleSchedule(SimpleScheduleKind.Second, 1, start.AddSeconds(1.8));
            await timeMachine.WaitUntilSecondsElapse(start, 1.4);
            job.Schedule = schedule2;

            await timeMachine.WaitUntilSecondsElapse(start, 14);

            //Assert.Pass(job.Output.ToString());
            //Assert.Pass($"idx: {idx}");

            //// Assert
            //Assert.That(dueTime1, Is.EqualTo(start.AddSeconds(2)));
            //Assert.That(dueTime2, Is.EqualTo(start.AddSeconds(1.8)));
        }
    }
}
