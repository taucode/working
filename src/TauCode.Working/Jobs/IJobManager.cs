﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        void Register(
            string jobName,
            Func<object, TextWriter, CancellationToken, Task> jobTaskCreator,
            ISchedule jobSchedule,
            object parameter);

        void SetParameter(string jobName, object parameter);

        object GetParameter(string jobName);

        void ChangeSchedule(string jobName, ISchedule newJobSchedule);

        void ChangeDueTime(string jobName, DateTime? dueTime);

        DueTimeInfo GetDueTime(string jobName);

        IList<DateTime> GetSchedulePart(string jobName, int length);

        void ManualStart(string jobName);

        void RedirectOutput(string jobName, TextWriter output);

        bool IsRunning(string jobName);

        void Cancel(string jobName);

        void Enable(string jobName, bool enable);

        bool IsEnabled(string jobName);

        JobInfo GetInfo(string jobName, bool includeLog);

        void Remove(string jobName);

        event EventHandler<JobChangedEventArgs> JobChanged;
    }
}
