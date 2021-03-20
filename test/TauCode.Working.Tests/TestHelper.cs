// todo clean up
namespace TauCode.Working.Tests
{
    internal static class TestHelper
    {

        //internal static async Task<bool> WaitUntilSecondsElapse(
        //    this ITimeProvider timeProvider,
        //    DateTimeOffset start,
        //    double seconds,
        //    CancellationToken token = default)
        //{
        //    var timeout = TimeSpan.FromSeconds(seconds);
        //    var now = timeProvider.GetCurrentTime();

        //    var elapsed = now - start;
        //    if (elapsed >= timeout)
        //    {
        //        //return false;
        //        throw new InvalidOperationException("Too late.");
        //    }

        //    while (true)
        //    {
        //        await Task.Delay(1, token);

        //        now = timeProvider.GetCurrentTime();

        //        elapsed = now - start;
        //        if (elapsed >= timeout)
        //        {
        //            return true;
        //        }
        //    }
        //}
    }
}
