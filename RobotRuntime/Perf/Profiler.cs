#define ENABLE_PROFILER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotRuntime.Perf
{
    public class Profiler
    {
        public static Profiler Instace { get { return m_Instance; } }
        private static Profiler m_Instance = new Profiler();
        private Profiler()
        {
            for (int i = 0; i < m_StopwatchCount; i++)
                m_FreeWatches.Add(new Stopwatch());
        }

        private const int StackLimit = 50;
        private const int m_StopwatchCount = 10;

        private Dictionary<string, LimitedStack<string>> m_Table = new Dictionary<string, LimitedStack<string>>();

        private Dictionary<string, Stopwatch> m_TakenWatches = new Dictionary<string, Stopwatch>();
        private LimitedStack<Stopwatch> m_FreeWatches = new LimitedStack<Stopwatch>(m_StopwatchCount);

        private object FreeWatchesLock = new object();
        private object TakenWatchesLock = new object();

        private void InstanceStart(string name)
        {
            LimitedStack<string> stack;
            if (m_Table.ContainsKey(name))
                stack = m_Table[name];
            else
            {
                stack = new LimitedStack<string>(StackLimit);
                m_Table.Add(name, stack);
            }

            Stopwatch watch;
            lock (FreeWatchesLock)
            {
                watch = m_FreeWatches.Pop();
            }

            lock (TakenWatchesLock)
            {
                m_TakenWatches.Add(name, watch);
            }

            watch.Start();
        }

        private void InstanceStop(string name)
        {
            if (!m_TakenWatches.ContainsKey(name))
                throw new InvalidOperationException("No start was called for this name: " + name);

            Stopwatch watch;
            lock (TakenWatchesLock)
            {
                watch = m_TakenWatches[name];
                m_TakenWatches.Remove(name);
            }

            if (m_TakenWatches.ContainsKey(name))
                throw new Exception("Watch should be removed for name: " + name);

            watch.Stop();
            var millis = watch.ElapsedMilliseconds;

            watch.Reset();

            lock (FreeWatchesLock)
            {
                m_FreeWatches.Add(watch);
            }

            m_Table[name].Add(name + ": " + millis + " ms.");
        }

        public static void Start(string name)
        {
#if ENABLE_PROFILER
            Profiler.Instace.InstanceStart(name);
#endif
        }

        public static void Stop(string name)
        {
#if ENABLE_PROFILER
            Profiler.Instace.InstanceStop(name);
#endif
        }

        public static void Begin(string name, Action action)
        {
            Start(name);
            action.Invoke();
            Stop(name);
        }
    }
}
