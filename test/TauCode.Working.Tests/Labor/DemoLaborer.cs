using System;
using TauCode.Working.Labor;

namespace TauCode.Working.Tests.Labor
{
    public class DemoLaborer : LaborerBase
    {
        public override bool IsPausingSupported => true;

        protected override void OnStarting()
        {
            throw new NotImplementedException();
        }

        protected override void OnStarted()
        {
            throw new NotImplementedException();
        }

        protected override void OnStopping()
        {
            throw new NotImplementedException();
        }

        protected override void OnStopped()
        {
            throw new NotImplementedException();
        }

        protected override void OnPausing()
        {
            throw new NotImplementedException();
        }

        protected override void OnPaused()
        {
            throw new NotImplementedException();
        }

        protected override void OnResuming()
        {
            throw new NotImplementedException();
        }

        protected override void OnResumed()
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposed()
        {
            throw new NotImplementedException();
        }
    }
}
