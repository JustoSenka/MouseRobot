using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using RobotRuntime;
using RobotRuntime.Logging;

namespace Tests
{
    class FakeLogger : ILogger
    {
        public IList<Log> LogList
        {
            get
            {
                return null;
            }
        }

        public event Action LogCleared;
        public event Action<Log> OnLogReceived;

        public void Clear()
        {
            LogCleared?.Invoke();
        }

        public void Logi(LogType logType, string str)
        {
            OnLogReceived?.Invoke(default(Log));
        }

        public void Logi(LogType logType, string obj, string description)
        {
            OnLogReceived?.Invoke(default(Log));
        }
    }
}
