using System.Diagnostics;

namespace RobotRuntime.Utils
{
    public class TraceListenerWhichLogs : TraceListener
    {
        public static void InsertAsFirstTraceListener(TraceListenerCollection traceListeners)
        {
            traceListeners.Insert(0, new TraceListenerWhichLogs());
        }

        public override void Fail(string message, string detailMessage)
        {
            Logger.Log(LogType.Error, message, detailMessage);
        }

        public override void Write(string message)
        {
            Logger.Log(LogType.Debug, message);
        }

        public override void WriteLine(string message)
        {
            // This will cause infinite recursion since logger writes to debug
            // Logger.Log(LogType.Debug, message);
        }
    }
}
