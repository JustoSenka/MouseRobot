using RobotRuntime.Tests;

namespace RobotRuntime.Execution
{
    /// <summary>
    /// This command will just forward call to Command.Run
    /// Compatible with all and any commands out there
    /// </summary>
    public class SimpleCommandRunner : IRunner
    {
        public TestData TestData { set; get; }

        public SimpleCommandRunner()
        {

        }

        public void Run(IRunnable runnable)
        {
            if (!(runnable is Command command))
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            TestData.InvokeCallback(command.Guid);
            command.Run(TestData);
        }
    }
}
