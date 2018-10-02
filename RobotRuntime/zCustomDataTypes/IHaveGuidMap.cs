using System;

namespace RobotRuntime
{
    public interface IHaveGuidMap
    {
        bool HasRegisteredGuid(Guid guid);
    }
}
