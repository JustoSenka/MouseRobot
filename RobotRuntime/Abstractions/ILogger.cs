using RobotRuntime.Logging;
using System;

namespace RobotRuntime.Abstractions
{
    public interface ILogger
    {
        event Action<Log> OnLogReceived;
        void Logi(LogType logType, string str);
        void Logi(LogType logType, string obj, string description);
    }
}