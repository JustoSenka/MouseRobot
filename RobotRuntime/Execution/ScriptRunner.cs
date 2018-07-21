using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System.Linq;

namespace RobotRuntime.Execution
{
    public class ScriptRunner : IRunner
    {
        public TestData TestData { set; get; }

        public ScriptRunner()
        {

        }

        public void Run(IRunnable runnable)
        {
            var script = runnable as LightScript;

            TestData.TestFixture = script;
            TestData.RunnerFactory.PassDependencies(TestData);

            Logger.Log(LogType.Debug, "Script is being run: " + script.Name);

            // making shallow copy of commands collection, so it doesn't crash when test tries to modify itself while running
            foreach (var node in script.Commands.ToList())
            {
                if (TestData.ShouldCancelRun)
                {
                    Logger.Log(LogType.Log, "Script run was cancelled.");
                    return;
                }

                var runner = TestData.RunnerFactory.GetFor(node.value.GetType());
                runner.Run(node.value);

                if (TestData.ShouldFailTest)
                    return;
            }
        }
    }
}
