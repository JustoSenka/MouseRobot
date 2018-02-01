using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RobotRuntime
{
    public partial class Logger : ILogger
    {
        public event Action<Log> OnLogReceived;
        public event Action LogCleared;

        public IList<Log> LogList { get; private set; } = new List<Log>();

        public Logger()
        {

        }

        public void Clear()
        {
            LogList = new List<Log>();
            LogCleared?.Invoke();
        }

        public void Logi(LogType logType, string str)
        {
            var i = this.IsTheCaller() ? 1 : 0; 
            InternalLog(logType, str, null, i + 2);
        }

        public void Logi(LogType logType, string str, string description)
        {
            var i = this.IsTheCaller() ? 1 : 0;
            InternalLog(logType, str, description, i + 2);
        }

        private void InternalLog(LogType logType, string obj, string description, int skipFrames)
        {
            var log = new Log(logType, obj, description, new StackTrace(skipFrames));

            var str = "[" + logType + "] " + log.Header;
            Console.WriteLine(str);
            Debug.WriteLine(str);

            LogList.Add(log);
            OnLogReceived?.Invoke(log);
        }
    }
}
