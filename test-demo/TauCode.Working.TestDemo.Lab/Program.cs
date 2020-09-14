using System;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.TestDemo.Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj = new object();

            Monitor.Enter(obj);
            Console.WriteLine("Main acquired obj");

            Task.Run( async () =>
            {
                await Task.Delay(200);

                Console.WriteLine("Task enters obj");
                Monitor.Enter(obj);


                Console.WriteLine("Task pulses obj");
                Monitor.Pulse(obj);


                Console.WriteLine("Task exits obj");
                Monitor.Exit(obj);
            });

            Console.WriteLine("Main waits obj");
            Monitor.Wait(obj);


            Console.WriteLine("Main got obj");
        }
    }
}
