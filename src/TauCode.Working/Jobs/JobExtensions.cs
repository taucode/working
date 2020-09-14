using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;

namespace TauCode.Working.Jobs
{
    public static class JobExtensions
    {
        private const int NeverYear = 9000;

        public static readonly DateTimeOffset Never;

        static JobExtensions()
        {
            Never = $"{NeverYear}-01-01Z".ToUtcDayOffset();
        }

        //public static bool IsNever(this DueTimeInfo dueTimeInfo) => dueTimeInfo.DueTime.Equals(Never);

        internal static Task IdleJobRoutine(
            object parameter,
            IProgressTracker progressTracker,
            TextWriter output,
            CancellationToken cancellationToken)
        {
            output?.WriteLine("Warning: usage of default idle routine.");
            return Task.CompletedTask;
        }
    }
}
