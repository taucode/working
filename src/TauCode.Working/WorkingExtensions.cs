using System;
using System.Collections.Generic;

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

        public static bool IsTransitionWorkerState(this WorkerState workerState) =>
            TransitionWorkerStates.Contains(workerState);

        public static bool IsStableWorkerState(this WorkerState workerState) =>
            StableWorkerStates.Contains(workerState);

        public static WorkerState GetStableWorkerState(this WorkerState transitionWorkerState)
        {
            var exists = Transitions.TryGetValue(transitionWorkerState, out var stableWorkerState);
            if (!exists)
            {
                throw new ArgumentException($"'{transitionWorkerState}' is not transition worker state.");
            }

            return stableWorkerState;
        }
    }
}
