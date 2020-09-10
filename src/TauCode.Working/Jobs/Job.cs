using System.IO;

namespace TauCode.Working.Jobs
{
    public class Job
    {
        public JobDelegate Routine { get; set; }

        public object Parameter { get; set; }

        public IProgressTracker ProgressTracker { get; set; }

        public TextWriter Output { get; set; }
    }
}
