using System;
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

        public bool HasDescription()
        {
            return Description != null && Description != "";
        }

        public bool CanBeExpanded()
        {
            return HasDescription() && LogType != LogType.None;
        }
    }
}

namespace RobotRuntime
{
    [Flags]
    public enum LogType
    {
        None = 0, Log = 1, Warning = 2, Error = 4, Debug = 8
    }
}