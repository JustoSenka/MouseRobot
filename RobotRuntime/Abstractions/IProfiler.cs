using RobotRuntime.Perf;
using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface IProfiler
    {
        int NodeLimit { get; }

        void Begin(string name, Action action);
        Dictionary<string, ProfilerNode[]> CopyNodes();
        void Start(string name);
        void Stop(string name);
    }
}