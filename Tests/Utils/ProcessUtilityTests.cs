using NUnit.Framework;
using RobotRuntime.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class ProcessUtilityTests
    {
        [Test]
        public void RunningCommandFromCommandLine_ReturnsConsoleOutput()
        {
            var result = ProcessUtility.StartFromCommandLine("cmd", "/c echo success");
            Assert.AreEqual("success", result);
        }
    }
}
