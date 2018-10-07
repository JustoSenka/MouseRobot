using System.Collections.Generic;

namespace RobotRuntime
{
    public class GuidEqualityComparer : IEqualityComparer<IHaveGuid>
    {
        public bool Equals(IHaveGuid x, IHaveGuid y)
        {
            return x.Guid == y.Guid;
        }

        public int GetHashCode(IHaveGuid obj)
        {
            return obj.Guid.GetHashCode();
        }
    }
}
