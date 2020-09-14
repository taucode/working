using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Labor.Exceptions;

namespace TauCode.Labor.TestDemo.Lab
{
    class Program
    {
        static Task Main(string[] args)
        {
            var cyc = new CycleProlBase();

            Task.Run(() =>
            {
                Thread.Sleep(100);

                try
                {
                    cyc.Start();
                }
                catch (InappropriateProlStateException ex)
                {
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] Failed to start: {ex}.");
                }
            });

            cyc.Start();
            Console.WriteLine("Started.");


            return Task.CompletedTask;
        }

    }
}
