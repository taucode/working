using System;

namespace TauCode.Working.Jobs
{
    public interface IProgressTracker
    {
        void UpdateProgress(decimal? percentCompleted, DateTimeOffset? estimatedEndTime);
    }
}
