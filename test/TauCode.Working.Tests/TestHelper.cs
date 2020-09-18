﻿using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs;

namespace TauCode.Working.Tests
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

        internal static void WaitUntil(this ITimeProvider timeProvider, DateTimeOffset moment)
        {
            var now = timeProvider.GetCurrent();
            if (now >= moment)
            {
                return;
            }

            while (true)
            {
                Thread.Sleep(1);

                now = timeProvider.GetCurrent();
                if (now >= moment)
                {
                    return;
                }
            }
        }

        internal static async Task WaitUntil(
            this ITimeProvider timeProvider,
            DateTimeOffset moment,
            CancellationToken token)
        {
            var now = timeProvider.GetCurrent();
            if (now >= moment)
            {
                return;
            }

            while (true)
            {
                await Task.Delay(1, token);

                now = timeProvider.GetCurrent();
                if (now >= moment)
                {
                    return;
                }
            }
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
