using NUnit.Framework;
using RobotRuntime.Utils;
using System.Diagnostics;

namespace Tests
{
    public class TestWithCleanup
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            Trace.Listeners.Clear();
            Debug.Listeners.Clear();
            Trace.Listeners.Add(new TraceListenerWhichLogs());
            Debug.Listeners.Add(new TraceListenerWhichLogs());
        }

        [OneTimeTearDown]
        public static void TryCleanUp()
        {
            TestUtils.TryCleanDirectory(TestUtils.TempFolderPath).Wait();
        }
    }
}
