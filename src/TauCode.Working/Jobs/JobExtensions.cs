using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Jobs
{
    public static class JobExtensions
    {
        private const int NeverYear = 9000;

        public static DateTime Never = new DateTime(NeverYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static Task IdleJobRoutine(object parameter,
            IProgressTracker progressTracker,
            TextWriter output,
            CancellationToken cancellationToken)
        {
            output?.WriteLine("Warning: usage of default idle routine.");
            return Task.CompletedTask;
        }
    }
}
