using NUnit.Framework;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;

namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public partial class JobTests
    {
        //private const int SetUpTimeout = 5000;
        private const int SetUpTimeout = 0;

        private StringWriterWithEncoding _logWriter;

        [SetUp]
        public async Task SetUp()
        {
            TimeProvider.Reset();
            GC.Collect();
            await Task.Delay(SetUpTimeout);

            _logWriter = new StringWriterWithEncoding(Encoding.UTF8);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TextWriter(_logWriter)
                .CreateLogger();
        }
    }
}
