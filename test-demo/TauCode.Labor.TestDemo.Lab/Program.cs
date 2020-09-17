using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
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

            var DEFECT = TimeSpan.FromMilliseconds(30);

            var now = "2000-01-01Z".ToUtcDayOffset();
            var timeMachine = ShiftedTimeProvider.CreateTimeMachine(now);
            TimeProvider.Override(timeMachine);

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await Task.Delay(1500, token); // 1.5 second to complete
            };
            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            job.IsEnabled = true;

            // Act
            job.Schedule = schedule; // will fire at 00:01

            await Task.Delay(
                1000 + // 0th due time
                DEFECT.Milliseconds +
                1500 +
                DEFECT.Milliseconds); // let job start, finish, and wait more 30 ms.

            // Assert
            var info = job.GetInfo(null);

            //Assert.That(info.CurrentRun, Is.Null);
            Debug.Assert(info.CurrentRun == null);

            //Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(3)));
            Debug.Assert(info.NextDueTime == now.AddSeconds(3));

            var pastRun = info.Runs.Single();

            //Assert.That(pastRun.RunIndex, Is.EqualTo(0));
            Debug.Assert(pastRun.RunIndex == 0);


            //Assert.That(pastRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            Debug.Assert(pastRun.StartReason == JobStartReason.ScheduleDueTime);

            //Assert.That(pastRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            Debug.Assert(pastRun.DueTime == (now.AddSeconds(1)));

            //Assert.That(pastRun.DueTimeWasOverridden, Is.False);
            Debug.Assert(pastRun.DueTimeWasOverridden == false);


            //Assert.That(pastRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(DEFECT));
            // todo


            //Assert.That(
            //    pastRun.EndTime,
            //    Is.EqualTo(pastRun.StartTime.AddSeconds(1.5)).Within(DEFECT));
            // todo

            //Assert.That(pastRun.Status, Is.EqualTo(JobRunStatus.Succeeded));

        }
    }
}
