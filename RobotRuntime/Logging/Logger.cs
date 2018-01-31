using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using System;
using System.Diagnostics;

namespace RobotRuntime
{
    public partial class Logger : ILogger
    {
        public event Action<Log> OnLogReceived;

        public Logger()
        {

        }

        public void Logi(LogType logType, string str)
        {
            Logi(logType, str, null);
        }

        public void Logi(LogType logType, string obj, string description)
        {
            var log = new Log(logType, obj, description, new StackTrace());

            var str = "[" + logType + "] " + log.Header;
            Console.WriteLine(str);
            Debug.WriteLine(str);

            OnLogReceived?.Invoke(log);
        }
    }
}
