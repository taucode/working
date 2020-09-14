using System;
using TauCode.Working.Jobs;
using TauCode.Working.Jobs.Omicron;

namespace TauCode.Labor.TestDemo.Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            var cnt = 10000;
            for (int i = 0; i < cnt; i++)
            {
                IJobManager jobManager = OmicronJobManager.CreateJobManager();
                jobManager.Start();

                // Act
                jobManager.Dispose();

                Console.WriteLine(i);
            }
        }
    }
}
