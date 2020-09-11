using System;
using System.Reflection;
using TauCode.Working.Jobs;

namespace TauCode.Working.Tests
{
    internal static class TestHelper
    {
        internal static void DebugPulseJobManager(this IJobManager jobManager)
        {
            var method = jobManager.GetType().GetMethod("DebugPulse", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new NotSupportedException();
            }

            method.Invoke(jobManager, new object[] { });
        }
    }
}
