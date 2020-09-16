using System;
using System.Collections.Generic;
using TauCode.Working.Jobs;

namespace TauCode.Working.ZetaOld.Jobs
{
    internal class ZetaJobInfoBuilder
    {
        internal ZetaJobInfoBuilder(/*string name*/)
        {
            //this.Name = name;
            this.Runs = new List<JobRunInfo>();
        }

        //internal string Name { get; }
        internal int RunCount { get; set; }
        //internal DueTimeInfo DueTimeInfo { get; set; }
        internal IList<JobRunInfo> Runs { get; }

        internal JobInfo Build()
        {
            throw new NotImplementedException();
            //return new JobInfo(
            //    //this.Name,
            //    null,
            //    this.DueTimeInfo,
            //    this.RunCount,
            //    this.Runs);
        }
    }
}
