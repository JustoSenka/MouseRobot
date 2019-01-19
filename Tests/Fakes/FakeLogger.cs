using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using RobotRuntime;
using RobotRuntime.Logging;
using System.Linq;

namespace Tests
{
    class FakeLogger : ILogger
    {
        public IList<Log> LogList
        {
            get
            {
                lock (m_LogLock)
                {
                    return m_LogList.ToList();
                }
            }
        }

        public event Action LogCleared;
        public event Action<Log> OnLogReceived;

        private IList<Log> m_LogList = new List<Log>();
        private object m_LogLock = new object();

        public void Clear()
        {
            lock (m_LogLock)
            {
                m_LogList = new List<Log>();
                LogCleared?.Invoke();
            }
        }

        public void Logi(LogType logType, string str)
        {
            Logi(logType, str, "");
        }

        public void Logi(LogType logType, string obj, string description)
        {
            var log = new Log(logType, obj, description, null);
            m_LogList.Add(log);

            var str = "[" + logType + "] " + log.Header;
            Console.WriteLine(str);

            OnLogReceived?.Invoke(log);
        }
    }
}
