using System;
using System.Collections.Generic;
using System.Linq;

namespace TauCode.Working.ZetaOld.Workers
{
    public static class ZetaWorkingExtensions
    {
        private static readonly HashSet<ZetaWorkerState> TransitionWorkerStates = new HashSet<ZetaWorkerState>(
            new List<ZetaWorkerState>
            {
                ZetaWorkerState.Stopping,
                ZetaWorkerState.Starting,
                ZetaWorkerState.Pausing,
                ZetaWorkerState.Resuming,
                ZetaWorkerState.Disposing,
            });

        private static readonly HashSet<ZetaWorkerState> StableWorkerStates = new HashSet<ZetaWorkerState>(
            new List<ZetaWorkerState>
            {
                ZetaWorkerState.Stopped,
                ZetaWorkerState.Running,
                ZetaWorkerState.Paused,
                ZetaWorkerState.Disposed,
            });

        private static readonly Dictionary<ZetaWorkerState, ZetaWorkerState> Transitions =
            new Dictionary<ZetaWorkerState, ZetaWorkerState>
            {
                { ZetaWorkerState.Stopping, ZetaWorkerState.Stopped },
                { ZetaWorkerState.Starting, ZetaWorkerState.Running },
                { ZetaWorkerState.Pausing, ZetaWorkerState.Paused },
                { ZetaWorkerState.Resuming, ZetaWorkerState.Running },
                { ZetaWorkerState.Disposing, ZetaWorkerState.Disposed },
            };

        internal static ZetaWorkerState[] NonDisposedStates = Enum
            .GetValues(typeof(ZetaWorkerState))
            .Cast<ZetaWorkerState>()
            .Except(new[]
            {
                ZetaWorkerState.Disposed,
                ZetaWorkerState.Disposing
            })
            .ToArray();


        public static bool IsTransitionWorkerState(this ZetaWorkerState workerState) =>
            TransitionWorkerStates.Contains(workerState);

        public static bool IsStableWorkerState(this ZetaWorkerState workerState) =>
            StableWorkerStates.Contains(workerState);

        public static ZetaWorkerState GetStableWorkerState(this ZetaWorkerState transitionWorkerState)
        {
            var exists = Transitions.TryGetValue(transitionWorkerState, out var stableWorkerState);
            if (!exists)
            {
                throw new ArgumentException($"'{transitionWorkerState}' is not a transition worker state.");
            }

            return stableWorkerState;
        }

        public static bool WorkerIsDisposed(this IZetaWorker worker)
        {
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            return worker.State == ZetaWorkerState.Disposed;
        }

        public static bool WorkerIsRunning(this IZetaWorker worker)
        {
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            return worker.State == ZetaWorkerState.Running;
        }

        public static bool WorkerIsStopped(this IZetaWorker worker)
        {
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            return worker.State == ZetaWorkerState.Stopped;
        }

    }
}
