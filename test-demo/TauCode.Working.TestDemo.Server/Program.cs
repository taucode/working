﻿using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using TauCode.Working.TestDemo.Server.Workers;

namespace TauCode.Working.TestDemo.Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();


            Console.Write("Worker type (1 - Timeout Worker): ");
            var workerType = Console.ReadLine();

            IWorker worker;

            switch (workerType)
            {
                case "1":
                    worker = new PersonTimeoutWorker();
                    break;

                default:
                    throw new NotImplementedException();
            }

            Console.Write("Worker name: ");
            var workerName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(workerName))
            {
                throw new NotImplementedException();
            }

            worker.Name = workerName;

            var configuration = CreateConfiguration();
            var connectionString = configuration["ConnectionStrings:RabbitMQ"];

            var wrapper = new WorkerWrapper(worker, connectionString);
            wrapper.Run();
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
