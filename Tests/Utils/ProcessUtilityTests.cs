using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotRuntime.Utils;

namespace Tests.Utils
{
    [TestClass]
    public class ProcessUtilityTests
    {
        [TestMethod]
        public void RunningCommandFromCommandLine_ReturnsConsoleOutput()
        {
            var result = ProcessUtility.StartFromCommandLine("cmd", "/c echo success");
            Assert.AreEqual("success", result);
        }
    }
}
