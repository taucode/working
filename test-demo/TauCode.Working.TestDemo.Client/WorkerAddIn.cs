using System;
using System.Collections.Generic;
using TauCode.Cli;

namespace TauCode.Working.TestDemo.Client
{
    /// <summary>
    /// Nameless add-in
    /// </summary>
    public class WorkerAddIn : CliAddInBase
    {
        public WorkerAddIn()
            : base(null, null, false)
        {
        }

        protected override void OnNodeCreated()
        {   
        }

        protected override IReadOnlyList<ICliWorker> CreateWorkers()
        {
            throw new NotImplementedException();

            //return new ICliWorker[]
            //{
            //    new StartWorkerWorker(),
            //};
        }
    }
}
