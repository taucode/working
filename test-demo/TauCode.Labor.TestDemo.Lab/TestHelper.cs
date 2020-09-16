using System;
using System.Threading;
using System.Threading.Tasks;
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

        // todo move to taucode.infra
        internal static async Task WaitUntil(DateTimeOffset now, DateTimeOffset moment, CancellationToken cancellationToken = default)
        {
            var timeout = moment - now;
            if (timeout < TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(timeout, cancellationToken);
        }

        //internal static IJobManager CreateJobManager() => JobManager.CreateJobManager();
        internal static IJobManager CreateJobManager() => JobManager.CreateJobManager();
    }
}
