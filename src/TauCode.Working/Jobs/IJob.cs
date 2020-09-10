using System.IO;

namespace TauCode.Working.Jobs
{
    public interface IJob
    {
        ISchedule Schedule { get; set; }
        JobDelegate Routine { get; set; }
        object Parameter { get; set; }
        IProgressTracker ProgressTracker { get; set; }
        TextWriter Output { get; set; }
    }
}
