using System;

namespace RobotRuntime
{
    public interface IHaveGuid
    {
        Guid Guid { get; }
        void RegenerateGuid();
        void OverrideGuid(Guid newGuid);
    }
}
