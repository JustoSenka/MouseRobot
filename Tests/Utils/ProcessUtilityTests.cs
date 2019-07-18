using NUnit.Framework;
using RobotRuntime;
using RobotRuntime.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class ProcessUtilityTests
    {
        [Test]
        public void RunningCommandFromCommandLine_ReturnsConsoleOutput()
        {
            Logger.Instance = new FakeLogger();

            var result = ProcessUtility.StartFromCommandLine("cmd", "/c echo success");
            Assert.AreEqual("success", result);
        }
    }
}
