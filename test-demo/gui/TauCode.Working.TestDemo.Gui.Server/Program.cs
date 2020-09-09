using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TauCode.Working.Jobs;
using TauCode.Working.TestDemo.Gui.Common;
using TauCode.Working.TestDemo.Gui.Server.Forms;

namespace TauCode.Working.TestDemo.Gui.Server
{
    public class Program
    {
        #region Static Entry Point

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [MTAThread]
        private static void Main()
        {
            var env = "Development";
            var configuration = CreateConfiguration(env);

            var program = new Program(configuration);
            program.Run();
        }

        private static IConfiguration CreateConfiguration(string env)
        {
            var mainSettingsFileName = "appsettings.json";
            var envSettingsFileName = $"appsettings.{env}.json";

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile(mainSettingsFileName, false, true)
                .AddJsonFile(envSettingsFileName, false);

            try
            {
                var configuration = configurationBuilder.Build();
                return configuration;
            }
            catch
            {
                Console.WriteLine($"ERROR: Failed to load configuration for environment '{env}'.");
                throw;
            }
        }

        #endregion
        
        private readonly IConfiguration _configuration;

        private Program(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void Run()
        {
            var writer = new TextBoxWriter();

            Log.Logger = new LoggerConfiguration()
                .Filter.ByIncludingOnly(x => x.Properties.ContainsKey("taucode.working"))
                .MinimumLevel.Debug()
                .WriteTo.TextWriter(writer)
                .CreateLogger();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            this.Bus = this.CreateBus();
            this.JobManager = this.CreateJobManager();
            this.JobManager.Start();

            this.MainForm = new MainForm(this.JobManager, writer);
            Application.Run(this.MainForm);

            this.Bus.Dispose();
            this.JobManager.Dispose();
        }

        private IJobManager CreateJobManager()
        {
            return new JobManager();
        }

        private IBus CreateBus()
        {
            var connectionString = _configuration["ConnectionStrings:RabbitMQ"];
            var bus = RabbitHutch.CreateBus(connectionString);
            return bus;
        }

        public MainForm MainForm { get; private set; }

        public IBus Bus { get; private set; }

        public IJobManager JobManager { get; private set; }

        public static Task CreateJobTask(object parameter, TextWriter writer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
