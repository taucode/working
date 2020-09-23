using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.TestDemo.Gui.Server
{
    public class DemoRunner
    {
        public async Task Run(object parameter, TextWriter writer, CancellationToken token)
        {
            var pars = (DemoRunnerParams)parameter;

            await writer.WriteLineAsync("Starting demo runner!");
            await writer.WriteLineAsync($"Count: {pars.Count} Timeout: {pars.Timeout}");

            for (var i = 0; i < pars.Count; i++)
            {
                await writer.WriteLineAsync($"Step {i} of {pars.Count}. Will sleep now.");

                try
                {
                    await Task.Delay(pars.Timeout, token);
                }
                catch (TaskCanceledException)
                {
                    await writer.WriteLineAsync("Got cancel signal! Canceling job.");
                    throw;
                }
            }
        }
    }
}
