using System;
using TauCode.Working.Jobs;

namespace TauCode.Working.Schedules
{
    internal sealed class NeverSchedule : ISchedule
    {
        private const string NeverDescription = "Technical schedule which due time never occurs";

        internal static ISchedule Instance = new NeverSchedule();

        private NeverSchedule()
        {   
        }

        public string Description
        {
            get => NeverDescription;
            set => throw new NotSupportedException();
        }

        public DateTimeOffset GetDueTimeAfter(DateTimeOffset after) => JobExtensions.Never; // todo: check argument
    }
}
