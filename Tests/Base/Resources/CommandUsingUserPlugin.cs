using System;
using RobotRuntime;
using RobotRuntime.Tests;

namespace CustomNamespace
{
    [Serializable]
    public class CommandLog : Command
    {
        public override string Name => "CommandLog";
        public override bool CanBeNested => false;

        public int Number;

        public CommandLog() { }

        public override void Run(TestData TestData)
        {
            Logger.Log(LogType.Log, "CommandLog: " + (Number + TestClassLibrary.Class.Method()));
        }

        public CommandLog(int number)
        {
            Number = number;
        }

        public override string ToString() => "CommandLog: " + Number;
    }
}
