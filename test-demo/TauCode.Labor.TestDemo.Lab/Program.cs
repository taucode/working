using System;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;
using TauCode.Working.Jobs.Omicron;
using TauCode.Working.Schedules;

namespace TauCode.Labor.TestDemo.Lab
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var cnt = 40;
            for (int i = 0; i < cnt; i++)
            {
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

            //using IJobManager jobManager = TestHelper.CreateJobManager();
            using IJobManager jobManager = OmicronJobManager.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                await Task.Delay(1500, token); // 1.5 second to complete
            };
            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            await Task.Delay(100);

            job.IsEnabled = true;

            // Act
            job.Schedule = schedule; // will fire at 00:01

            await Task.Delay(
                1000 + // 0th due time
                DEFECT.Milliseconds +
                1500 +
                DEFECT.Milliseconds); // let job start, finish, and wait more 20 ms.

            // Assert
            var info = job.GetInfo(null);

            if (info.CurrentRun == null && info.NextDueTime == now.AddSeconds(1))
            {
                Console.WriteLine("*** BAD ***");
            }
            else
            {
                Console.WriteLine("Good :)");
            }

            //Assert.That(info.CurrentRun, Is.Null);
            //Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(2)));

            //var pastRun = info.Runs.Single();

            //Assert.That(pastRun.RunIndex, Is.EqualTo(0));
            //Assert.That(pastRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            //Assert.That(pastRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            //Assert.That(pastRun.DueTimeWasOverridden, Is.False);

            //Assert.That(pastRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(DEFECT));
            //Assert.That(
            //    pastRun.EndTime,
            //    Is.EqualTo(pastRun.StartTime.AddSeconds(1.5)).Within(DEFECT));

            //Assert.That(pastRun.Status, Is.EqualTo(JobRunStatus.Succeeded));

            //Console.WriteLine("- About to dispose");
            jobManager.Dispose();
        }
    }
}
