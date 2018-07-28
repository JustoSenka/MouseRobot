using System.Collections.Generic;

namespace RobotRuntime
{
    public class SimilarEqualityComparer : IEqualityComparer<ISimilar>
    {
        public bool Equals(ISimilar x, ISimilar y)
        {
            if (x == null)
                return x == y;

            return x.Similar(y);
        }

        public int GetHashCode(ISimilar obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }
    }
}
