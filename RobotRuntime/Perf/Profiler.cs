#define ENABLE_PROFILER
#define ENABLE_PROFILER_DEBUGGING

using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Lifetime;

namespace RobotRuntime.Perf
{
    [RegisterTypeToContainer(typeof(IProfiler), typeof(ContainerControlledLifetimeManager))]
    public class Profiler : IProfiler
    {
        public Profiler()
        {
            for (int i = 0; i < m_StopwatchCount; i++)
                m_FreeWatches.Add(new Stopwatch());
        }

        public int NodeLimit { get { return 50; } }
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

        private void InternalStart(string name)
        {
#if ENABLE_PROFILER_DEBUGGING
            if (m_TakenWatches.ContainsKey(name))
            {
                Logger.Log(LogType.Error, "No Stop was called for name: " + name);
                InternalStop(name);
            }
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
                Logger.Log(LogType.Error, "No free watches left, maybe try increasing watch count?");
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

        private void InternalStop(string name)
        {
#if ENABLE_PROFILER_DEBUGGING
            if (!m_TakenWatches.ContainsKey(name))
            {
                Logger.Log(LogType.Error, "No start was called for this name: " + name);
                return;
            }
#endif

            Stopwatch watch;
            lock (TakenWatchesLock)
            {
                watch = m_TakenWatches[name];
                m_TakenWatches.Remove(name);
            }

#if ENABLE_PROFILER_DEBUGGING
            if (m_TakenWatches.ContainsKey(name))
                Logger.Log(LogType.Error, "Watch should be removed for name: " + name);
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

        public void Start(string name)
        {
#if ENABLE_PROFILER
            InternalStart(name);
#endif
        }

        public void Stop(string name)
        {
#if ENABLE_PROFILER
            InternalStop(name);
#endif
        }

        public void Begin(string name, Action action)
        {
            using (var sample = new ProfilerSample(this, name))
            {
                action.Invoke();
            }
        }

        private class ProfilerSample : IDisposable
        {
            private IProfiler Profiler;
            private string m_Name;

            public ProfilerSample(IProfiler Profiler, string name)
            {
                this.Profiler = Profiler;
                this.m_Name = name;

                Profiler.Start(m_Name);
            }

            public void Dispose()
            {
                Profiler.Stop(m_Name);
            }
        }
    }
}
