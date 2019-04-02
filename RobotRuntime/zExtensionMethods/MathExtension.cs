using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime
{
    public static class MathExtension
    {
        /// <summary>
        /// Returns true if numbers are consecutive
        /// Based on this algorithm: https://www.geeksforgeeks.org/check-if-array-elements-are-consecutive/
        /// O(n) complexity
        /// </summary>
        public static bool AreNumbersConsecutive(this IEnumerable<int> enumerable)
        {
            var arr = enumerable.ToArray();
            var n = arr.Length;
            int min = arr.Min();
            int max = arr.Max();
            if (max - min + 1 == n)
            {
                var visited = new bool[n];

                for (var i = 0; i < n; i++)
                {
                    /* If we see an element again, then 
                    return false */
                    if (visited[arr[i] - min] != false)
                        return false;

                    /* If visited first time, then mark  
                    the element as visited */
                    visited[arr[i] - min] = true;
                }

                /* If all elements occur once, then 
                return true */
                return true;
            }
            return false;
        }
    }
}
