using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TauCode.Working
{
    public static class WorkingExtensions
    {
        private static readonly HashSet<WorkerState> TransitionWorkerStates = new HashSet<WorkerState>(
            new List<WorkerState>
            {
                WorkerState.Stopping,
                WorkerState.Starting,
                WorkerState.Pausing,
                WorkerState.Resuming,
                WorkerState.Disposing,
            });

        private static readonly HashSet<WorkerState> StableWorkerStates = new HashSet<WorkerState>(
            new List<WorkerState>
            {
                WorkerState.Stopped,
                WorkerState.Running,
                WorkerState.Paused,
                WorkerState.Disposed,
            });

        private static readonly Dictionary<WorkerState, WorkerState> Transitions =
            new Dictionary<WorkerState, WorkerState>
            {
                { WorkerState.Stopping, WorkerState.Stopped },
                { WorkerState.Starting, WorkerState.Running },
                { WorkerState.Pausing, WorkerState.Paused },
                { WorkerState.Resuming, WorkerState.Running },
                { WorkerState.Disposing, WorkerState.Disposed },
            };

        internal static WorkerState[] NonDisposedStates = Enum
            .GetValues(typeof(WorkerState))
            .Cast<WorkerState>()
            .Except(new[]
            {
                WorkerState.Disposed,
                WorkerState.Disposing
            })
            .ToArray();


        public static bool IsTransitionWorkerState(this WorkerState workerState) =>
            TransitionWorkerStates.Contains(workerState);

        public static bool IsStableWorkerState(this WorkerState workerState) =>
            StableWorkerStates.Contains(workerState);

        public static WorkerState GetStableWorkerState(this WorkerState transitionWorkerState)
        {
            var exists = Transitions.TryGetValue(transitionWorkerState, out var stableWorkerState);
            if (!exists)
            {
                throw new ArgumentException($"'{transitionWorkerState}' is not a transition worker state.");
            }

            return stableWorkerState;
        }

        //internal static DateTime Trunca-teMilliseconds(this DateTime dateTime)
        //{
        //    return new DateTime(
        //        dateTime.Year,
        //        dateTime.Month,
        //        dateTime.Day,
        //        dateTime.Hour,
        //        dateTime.Minute,
        //        dateTime.Second,
        //        0,
        //        dateTime.Kind);
        //}


        // todo temp
        public static string FormatTime(this DateTime dateTime)
        {
            return dateTime.ToString("O", CultureInfo.InvariantCulture);
        }

        public static bool IsWorkerDisposed(this IWorker worker)
        {
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            return worker.State == WorkerState.Disposed;
        }

        public static bool IsWorkerRunning(this IWorker worker)
        {
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            return worker.State == WorkerState.Running;
        }
    }
}
