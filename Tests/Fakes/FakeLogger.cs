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

        public static IList<Log> CreateLogsFromConsoleOutput(string text)
        {
            var lines = text.Split('\n');

            // I build enum string - value map because parsing for enums is really slow: Enum.TryParse(logTypeStr, out logType); // 3 ms
            var values = Enum.GetValues(typeof(LogType)).Cast<LogType>();
            var map = new Dictionary<string, LogType>(values.Count());
            foreach (var value in values)
                map.Add(value.ToString(), value);

            return lines.Select(line =>
            {
                var colls = line.Split(new[] { "] " }, StringSplitOptions.None);
                var logTypeStr = colls[0].Trim('[', '\n', '\r');
                var header = colls[1].Trim(' ', '\n', '\r');
                var logType = map[logTypeStr];

                return new Log() { LogType = logType, Header = header};
            }).ToList();
        }
    }
}
