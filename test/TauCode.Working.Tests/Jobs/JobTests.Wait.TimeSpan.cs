using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace TauCode.Working.Tests.Jobs
{
    [TestFixture]
    public partial class JobTests
    {
        // todo - was running, then ends => waits and returns true
        [Test]
        public async Task WaitTimeSpan_WasRunningThenEnds_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - negative arg, throws
        [Test]
        public async Task WaitTimeSpan_NegativeArgument_ThrowsTodo()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then canceled => waits and returns true
        [Test]
        public async Task WaitTimeSpan_WasRunningThenCanceled_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then faulted => waits and returns true
        [Test]
        public async Task WaitTimeSpan_WasRunningThenFaulted_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then job disposed => waits and returns true
        [Test]
        public async Task WaitTimeSpan_WasRunningThenJobIsDisposed_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, then job manager disposed => waits and returns true
        [Test]
        public async Task WaitTimeSpan_WasRunningThenJobManagerIsDisposed_WaitsAndReturnsTrue()
        {
            throw new NotImplementedException();
        }

        // todo - was running, timeout => returns false
        [Test]
        public async Task WaitTimeSpan_WasRunningTooLong_WaitsAndReturnsFalse()
        {
            throw new NotImplementedException();
        }

        // todo - not running => returns true immediately
        [Test]
        public async Task WaitTimeSpan_NotRunning_ReturnsTrueImmediately()
        {
            throw new NotImplementedException();
        }

        // todo - was disposed => throws
        [Test]
        public async Task WaitTimeSpan_JobIsDisposed_ThrowsTodo()
        {
            throw new NotImplementedException();
        }
    }
}
