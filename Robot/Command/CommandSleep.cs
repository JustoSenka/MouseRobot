using System;
using System.Threading;

namespace Robot
{
    [Serializable]
    class CommandSleep : Command
    {
        public int Time { get; set; }

        public CommandSleep(int millis)
        {
            Time = millis;
            Text = "Sleep for " + millis + " ms.";
        }

        public override object Clone()
        {
            return new CommandSleep(Time);
        }

        public override void Run()
        {
            Thread.Sleep(Time);
        }
    }
}
