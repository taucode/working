﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Labor
{
    public abstract class QueueLaborerBase<TAssignment> : LoopLaborerBase
    {
        private readonly Queue<TAssignment> _assignments;
        private readonly object _lock;

        protected QueueLaborerBase()
        {
            _assignments = new Queue<TAssignment>();
            _lock = new object();
        }

        public void AddAssignment(TAssignment assignment)
        {
            if (this.IsDisposed)
            {
                throw this.CreateObjectDisposedException(nameof(AddAssignment));
            }

            var state = this.State;
            if (state == LaborerState.Stopped || state == LaborerState.Stopping)
            {
                throw this.CreateInvalidLaborerOperationException(nameof(AddAssignment), state);
            }

            this.CheckAssignment(assignment);

            lock (_lock)
            {
                _assignments.Enqueue(assignment);
            }

            this.AbortVacation();
        }

        protected abstract Task DoAssignment(TAssignment assignment, CancellationToken cancellationToken);

        protected virtual void CheckAssignment(TAssignment assignment)
        {
            // idle.
        }

        protected override void OnStopped()
        {
            base.OnStopped();

            lock (_lock)
            {
                _assignments.Clear();
            }
        }

        public override bool IsPausingSupported => true;

        protected override async Task<TimeSpan> DoLabor(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                TAssignment assignment;

                lock (_lock)
                {
                    if (_assignments.Count > 0)
                    {
                        assignment = _assignments.Dequeue();
                    }
                    else
                    {
                        return VeryLongVacation; // no assignments to work on.
                    }
                }

                try
                {
                    await this.DoAssignment(assignment, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw; // task was canceled for reason
                }
                catch (Exception ex)
                {
                    this.GetSafeLogger().LogError(ex, "Exception thrown while processing the assignment.");
                }
            }
        }
    }
}