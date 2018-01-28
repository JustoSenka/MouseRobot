#define ENABLE_PROFILER
#define ENABLE_PROFILER_DEBUGGING

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
        public static Profiler Instance { get { return m_Instance; } }
        private static Profiler m_Instance = new Profiler();
        private Profiler()
        {
            for (int i = 0; i < m_StopwatchCount; i++)
                m_FreeWatches.Add(new Stopwatch());
        }

        public const int NodeLimit = 50;
        private const int m_StopwatchCount = 10;

        private Dictionary<string, LimitedStack<ProfilerNode>> m_Table = new Dictionary<string, LimitedStack<ProfilerNode>>();
        private object m_TableLock = new object();

        public Dictionary<string, ProfilerNode[]> CopyNodes()
        {
            var newTable = new Dictionary<string, ProfilerNode[]>();
            lock (m_TableLock)
            {
                foreach (var pair in m_Table)
                {
                    newTable.Add(pair.Key, pair.Value.ToArray());
                }
            }
            return newTable;
        }

        private Dictionary<string, Stopwatch> m_TakenWatches = new Dictionary<string, Stopwatch>();
        private LimitedStack<Stopwatch> m_FreeWatches = new LimitedStack<Stopwatch>(m_StopwatchCount);

        private object FreeWatchesLock = new object();
        private object TakenWatchesLock = new object();

        private void InstanceStart(string name)
        {
#if ENABLE_PROFILER_DEBUGGING
            if (m_TakenWatches.ContainsKey(name))
                throw new InvalidOperationException("No Stop was called for name: " + name);
#endif

            LimitedStack<ProfilerNode> stack;

            if (m_Table.ContainsKey(name))
                stack = m_Table[name];
            else
            {
                stack = new LimitedStack<ProfilerNode>(NodeLimit);
                lock (m_TableLock)
                {
                    m_Table.Add(name, stack);
                }
            }

#if ENABLE_PROFILER_DEBUGGING
            if (m_FreeWatches.Count() == 0)
                throw new Exception("No free watches left, maybe try increasing watch count?");
#endif

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
#if ENABLE_PROFILER_DEBUGGING
            if (!m_TakenWatches.ContainsKey(name))
                throw new InvalidOperationException("No start was called for this name: " + name);
#endif

            Stopwatch watch;
            lock (TakenWatchesLock)
            {
                watch = m_TakenWatches[name];
                m_TakenWatches.Remove(name);
            }

#if ENABLE_PROFILER_DEBUGGING
            if (m_TakenWatches.ContainsKey(name))
                throw new Exception("Watch should be removed for name: " + name);
#endif

            watch.Stop();
            var millis = watch.ElapsedMilliseconds;

            watch.Reset();

            lock (FreeWatchesLock)
            {
                m_FreeWatches.Add(watch);
            }

            lock (m_TableLock)
            {
                m_Table[name].Add(new ProfilerNode(name, (int)millis/* + " ms."*/));
            }
        }

        public static void Start(string name)
        {
#if ENABLE_PROFILER
            Profiler.Instance.InstanceStart(name);
#endif
        }

        public static void Stop(string name)
        {
#if ENABLE_PROFILER
            Profiler.Instance.InstanceStop(name);
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
