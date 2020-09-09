using System;
using System.IO;
using EasyNetQ;
using Microsoft.Extensions.Configuration;

namespace TauCode.Working.TestDemo.Cui.Client
{
    public class Program
    {
        private static int Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Information()
            //    .WriteTo.Console()
            //    .CreateLogger();

            var configuration = CreateConfiguration();
            var connectionString = configuration["ConnectionStrings:RabbitMQ"];

            var bus = RabbitHutch.CreateBus(connectionString);

            var runner = new WorkerHostRunner(bus);
            var res = runner.Run(args);

            bus.Dispose();

            return res;
        }

        private static IConfiguration CreateConfiguration()
        {
            var confFileName = "appsettings.json";
            var defaultConfFileName = "appsettings.Development.json";


            // todo[temp]
            confFileName = defaultConfFileName;

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile(confFileName, false, true);

            IConfigurationRoot configuration;

            try
            {
                configuration = configurationBuilder.Build();
                return configuration;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine($"Failed to load '{confFileName}'. Defaulting to '{defaultConfFileName}'.");

                configurationBuilder = new ConfigurationBuilder()
                    .AddJsonFile(defaultConfFileName, false, true);
                configuration = configurationBuilder.Build();
                return configuration;
            }
        }
    }
}
