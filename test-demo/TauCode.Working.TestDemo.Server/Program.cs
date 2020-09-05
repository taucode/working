﻿using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Working.TestDemo.Server.Workers;

namespace TauCode.Working.TestDemo.Server
{
    public class Program
    {
        private readonly IConfiguration _configuration;
        private IBus _bus;

        private static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Filter.ByIncludingOnly(x => x.Properties.ContainsKey("taucode.working"))
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var configuration = CreateConfiguration();
            //

            var program = new Program(configuration);
            await program.Run(args);
        }

        public Program(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Run(string[] args)
        {
            var connectionString = _configuration["ConnectionStrings:RabbitMQ"];
            _bus = RabbitHutch.CreateBus(connectionString);

            var firstIteration = true;

            while (true)
            {
                string input;
                string name;

                if (firstIteration && args.Length == 2)
                {
                    input = "1";
                    name = "a";
                }
                else
                {
                    this.WritePrompt();
                    input = Console.ReadLine();
                    name = this.ReadName();
                }

                firstIteration = false;

                if (!this.IsValidInput(input))
                {
                    continue;
                }

                if (input == "0")
                {
                    break;
                }

                var worker = this.CreateWorker(input, name);
                var wrapper = new WorkerWrapper(worker, _bus);

                Console.WriteLine($"Initializing worker with type {worker.GetType().FullName} and name '{worker.Name}'.");

                await wrapper.Run();
            }

            _bus.Dispose();
        }

        private string ReadName()
        {
            while (true)
            {
                Console.Write("Worker name: ");
                var name = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                return name;
            }
        }

        private IRabbitWorker CreateWorker(string workerType, string workerName)
        {
            switch (workerType)
            {
                case "1":
                    return new SimpleTimeoutWorker(_bus)
                    {
                        Name = workerName,
                    };

                default:
                    throw new ArgumentException();
            }
        }

        private bool IsValidInput(string input)
        {
            return input.IsIn("0", "1");
        }

        private void WritePrompt()
        {
            Console.Write(@"
Choose worker type or exit
0 - Exit
1 - Timeout Worker
: ");
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
