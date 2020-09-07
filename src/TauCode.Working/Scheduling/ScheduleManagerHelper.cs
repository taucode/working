using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Scheduling
{
    internal class ScheduleManagerHelper : LoopWorkerBase
    {
        #region Overridden

        protected override Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override IList<AutoResetEvent> CreateExtraSignals()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
