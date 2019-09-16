using NUnit.Framework;
using RobotRuntime.Utils;
using System.Diagnostics;

// This will run one test from each fixture at the same time
// Environment.CurrentDirectory is the problem right now, because most tests need a project running
// [assembly: Parallelizable(ParallelScope.Fixtures)]

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
