using NUnit.Framework;
using TauCode.Infrastructure.Time;

namespace TauCode.Working.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        protected ITimeProvider MyTimeProvider { get; set; } = new UtcTimeProvider();

        //protected IJobManager CreateJobManager() => 
        
    }
}
