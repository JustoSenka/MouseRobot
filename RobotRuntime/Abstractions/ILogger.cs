using RobotRuntime.Logging;
using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface ILogger
    {
        event Action<Log> OnLogReceived;
        event Action LogCleared;

        IList<Log> LogList { get; }

        void Clear();
        void Logi(LogType logType, string str);
        void Logi(LogType logType, string obj, string description);
    }
}