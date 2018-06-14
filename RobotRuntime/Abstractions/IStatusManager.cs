using RobotRuntime.Logging;
using System;
using System.Drawing;

namespace RobotRuntime.Abstractions
{
    public interface IStatusManager
    {
        Status Status { get; }

        void Add(string uniqueName, int priority, Status status);
        void Remove(string uniqueName);

        event Action<Status> AnimationUpdated;
        event Action<Status> StatusUpdated;
    }
}