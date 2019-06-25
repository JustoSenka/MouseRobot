using System.Collections.Generic;

namespace RobotRuntime
{
    public class TesseractStringEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return Convert(x).Equals(Convert(y), System.StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return Convert(obj).GetHashCode();
        }

        public string Convert(string str)
        {
            return str.Replace('I', 'l').Replace('|', 'l').ToLowerInvariant();
        }
    }
}
