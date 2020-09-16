using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
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
            var fakeNow = "2020-01-01Z".ToUtcDayOffset();
            TimeProvider.Override(ShiftedTimeProvider.CreateTimeMachine(fakeNow));

            using IJobManager jobManager = TestHelper.CreateJobManager();
            jobManager.Start();

            var job1 = jobManager.Create("job1");
            job1.IsEnabled = true;

            //var job2 = jobManager.Create("job2");
            //job2.IsEnabled = true;

            job1.Output = new StringWriterWithEncoding(Encoding.UTF8);
            //job2.Output = new StringWriterWithEncoding(Encoding.UTF8);

            async Task Routine(object parameter, IProgressTracker tracker, TextWriter output, CancellationToken token)
            {
                for (var i = 0; i < 100; i++)
                {
                    var time = TimeProvider.GetCurrent();
                    await output.WriteLineAsync($"Iteration {i}: {time.Second:D2}:{time.Millisecond:D3}");

                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        time = TimeProvider.GetCurrent();
                        await output.WriteLineAsync($"Canceled! {time.Second:D2}:{time.Millisecond:D3}");
                        throw;
                    }
                }
            }

            ISchedule schedule = new SimpleSchedule(
                SimpleScheduleKind.Second,
                1,
                fakeNow.AddMilliseconds(400));


            job1.Schedule = schedule;
            //job2.Schedule = schedule;

            job1.Routine = Routine;
            //job2.Routine = Routine;

            await Task.Delay(2500); // 3 iterations should be completed: ~400, ~1400, ~2400 todo: ut this

            // Act
            //var jobInfoBeforeDispose1 = job1.GetInfo(null);
            //var jobInfoBeforeDispose2 = job2.GetInfo(null);

            jobManager.Dispose();

            // Assert
            Debug.Assert(jobManager.IsDisposed);
            //Assert.That(jobManager.IsDisposed, Is.True);

            foreach (var job in new[] { job1, /*job2*/ })
            {
                Debug.Assert(job.IsDisposed);

                await Task.Delay(250); // let task finish.
                
                //Assert.That(job.IsDisposed, Is.True);
                

                Console.WriteLine("~GetInfo");
                var info = job.GetInfo(null);
                var run = info.Runs.Single();

                //Assert.That(run.Status, Is.EqualTo(JobRunStatus.Canceled));
                Debug.Assert(run.Status == JobRunStatus.Canceled);
            }
        }
    }
}
