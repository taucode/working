using System;
using System.Linq;
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

            using IJobManager jobManager = OmicronJobManager.CreateJobManager();
            jobManager.Start();
            var job = jobManager.Create("my-job");

            job.Routine = async (parameter, tracker, output, token) =>
            {
                for (int i = 0; i < 15; i++)
                {
                    Console.WriteLine($"Hello {i}!");
                    await Task.Delay(100, token);
                }
            };
            ISchedule schedule = new SimpleSchedule(SimpleScheduleKind.Second, 1, now);

            job.IsEnabled = true;

            // Act
            job.Schedule = schedule; // will fire at 00:01


            await Task.Delay(1400 + DEFECT.Milliseconds);
            var canceled = job.Cancel(); // will be canceled almost right after start

            //Assert.That(canceled, Is.True);

            // Assert
            var info = job.GetInfo(null);
            //Assert.That(info.CurrentRun, Is.Null);
            //Assert.That(info.NextDueTime, Is.EqualTo(now.AddSeconds(2)));

            var pastRun = info.Runs.Single();

            //Assert.That(pastRun.RunIndex, Is.EqualTo(0));
            //Assert.That(pastRun.StartReason, Is.EqualTo(JobStartReason.ScheduleDueTime));
            //Assert.That(pastRun.DueTime, Is.EqualTo(now.AddSeconds(1)));
            //Assert.That(pastRun.DueTimeWasOverridden, Is.False);

            //Assert.That(pastRun.StartTime, Is.EqualTo(now.AddSeconds(1)).Within(DEFECT));
            //Assert.That(
            //    pastRun.EndTime,
            //    Is.EqualTo(pastRun.StartTime.AddSeconds(0)).Within(DEFECT * 2));

            //Assert.That(pastRun.Status, Is.EqualTo(JobRunStatus.Canceled));
            if (pastRun.Status != JobRunStatus.Canceled)
            {
                throw new NotImplementedException();
            }
        }
    }
}
