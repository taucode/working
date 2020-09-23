using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;

namespace TauCode.Labor.TestDemo.Lab
{
    internal static class TestHelper
    {
        //internal static void DebugPulseJobManager(this IJobManager jobManager)
        //{
        //    var method = jobManager.GetType().GetMethod("DebugPulse", BindingFlags.NonPublic | BindingFlags.Instance);
        //    if (method == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    method.Invoke(jobManager, new object[] { });
        //}

        // todo move to taucode.infra?
        internal static async Task WaitUntil(DateTimeOffset now, DateTimeOffset moment, CancellationToken cancellationToken = default)
        {
            var timeout = moment - now;
            if (timeout < TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(timeout, cancellationToken);
        }

        internal static IJobManager CreateJobManager()
        {
            var jobManager = new JobManager();
            JobDiagnosticsHelper.EnableLogging(jobManager, true);
            return jobManager;
        }


        internal static async Task WaitUntilSecondsElapse(
            this ITimeProvider timeProvider,
            DateTimeOffset start,
            double seconds,
            CancellationToken token = default)
        {
            var timeout = TimeSpan.FromSeconds(seconds);
            var now = timeProvider.GetCurrent();

            var elapsed = now - start;
            if (elapsed >= timeout)
            {
                return;
            }

            while (true)
            {
                await Task.Delay(1, token);

                now = timeProvider.GetCurrent();

                elapsed = now - start;
                if (elapsed >= timeout)
                {
                    return;
                }
            }
        }

    }
}
