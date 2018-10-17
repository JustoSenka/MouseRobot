using System.Collections.Generic;

namespace RobotRuntime
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Adds value to dictionary or overrides existing one by matching key.
        /// </summary>
        /// <returns>True if value was overriden</returns>
        public static bool AddOrOverride<T, K>(this Dictionary<T, K> dict, T key, K value)
        {
            var containsKey = dict.ContainsKey(key);
            if (containsKey)
                dict[key] = value;
            else
                dict.Add(key, value);

            return containsKey;
        }

        /// <summary>
        /// Adds value to dictionary or overrides existing one by matching key.
        /// </summary>
        /// <returns>True if key was removed</returns>
        public static bool RemoveIfExists<T, K>(this Dictionary<T, K> dict, T key)
        {
            var containsKey = dict.ContainsKey(key);
            if (containsKey)
                dict.Remove(key);

            return containsKey;
        }
    }
}
