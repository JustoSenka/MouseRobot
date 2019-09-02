using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotRuntime.Reflection;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.Lifetime;

namespace RobotRuntime
{
    [RegisterTypeToContainer(typeof(ILogger), typeof(ContainerControlledLifetimeManager))]
    public partial class Logger : MarshalByRefObject, ILogger
    {
        public event Action<Log> OnLogReceived;
        public event Action LogCleared;

        private IList<Log> m_LogList = new List<Log>();

        /// <summary>
        /// Creating new list so original is not modified. Also makes it thread safe.
        /// </summary>
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

        private string LogName { get { return "Log.txt"; } }
        private string LogPath { get { return Path.Combine(Paths.RoamingAppdataPath, LogName); } }

        private object m_LogLock = new object();

        public Logger()
        {
            lock (m_LogLock)
            {
                if (File.Exists(LogPath))
                    File.Delete(LogPath);

                if (!Directory.Exists(Paths.RoamingAppdataPath))
                    Directory.CreateDirectory(Paths.RoamingAppdataPath);

                File.Create(LogPath);
            }
        }

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
            lock (m_LogLock)
            {
                var log = new Log(logType, obj, description, new StackTrace(skipFrames));

                var str = "[" + logType + "] " + log.Header;
                Console.WriteLine(str);
                Debug.WriteLine(str);

                m_LogList.Add(log);

                var strToFile = log.HasDescription() ? str + Environment.NewLine + description : str;
                var multilineStr = new string[] { strToFile, log.Stacktrace.ToString() + Environment.NewLine };

                try
                {
                    File.AppendAllLines(LogPath, multilineStr);
                }
                catch (IOException) { }

                OnLogReceived?.Invoke(log);
            }
        }
    }
}
