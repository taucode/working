using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace TauCode.Working.Tests.Labor
{
    [TestFixture]
    public class LoopLaborerTests
    {
        private StringLogger _logger;

        private string CurrentLog => _logger.ToString();

        [SetUp]
        public async Task SetUp()
        {
            _logger = new StringLogger();
            await Task.Delay(5); // let TPL initiate
        }

        [Test]
        public async Task TodoFoo()
        {
            // Arrange
            var laborer = new DemoLoopLaborer
            {
                Name = "Psi",
            };

            laborer.Logger = _logger;
            laborer.LaborAction = async (@base, token) =>
            {
                @base.Logger.LogInformation("hello");
                await Task.Delay(100, token);
                return TimeSpan.FromMilliseconds(200);
            };

            // Act
            laborer.Start();
            await Task.Delay(400);
            laborer.Stop();

            // Assert
            laborer.Dispose();

            throw new NotImplementedException();
        }
    }
}
