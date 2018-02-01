using System.Diagnostics;

namespace RobotRuntime.Logging
{
    public struct Log
    {
        public string Header { get; set; }
        public string Description { get; set; }
        public StackTrace Stacktrace { get; set; }
        public LogType LogType { get; set; }

        public Log(LogType LogType, string Header, string Description, StackTrace Stacktrace)
        {
            this.Header = Header;
            this.Description = Description;
            this.Stacktrace = Stacktrace;
            this.LogType = LogType;
        }
    }
}

namespace RobotRuntime
{
    public enum LogType
    {
        None = 0, Error, Warning, Log, Debug
    }
}