using RobotRuntime.Recordings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime
{
    public static class IHaveGuidExtension
    {
        /// <summary>
        /// Adds guid to map. If map already contains guid, regenerate and print warning.
        /// Do not use this method if it is expeced for map to have duplicate guid.
        /// Regenerate is only supposed to happen on exceptions so it doesn't crash.
        /// </summary>
        public static void AddGuidToMapAndGenerateUniqueIfNeeded(this HashSet<Guid> map, IHaveGuid objectWithGuid)
        {
            if (map.Contains(objectWithGuid.Guid))
            {
                Logger.Log(LogType.Warning, "Object '" + objectWithGuid.ToString() + "' has duplicate guid. Regenerating.");
                objectWithGuid.RegenerateGuid();
            }

            map.Add(objectWithGuid.Guid);
        }

        /// <summary>
        /// Safely removes guid from map. If guid does not exist, does nothing.
        /// </summary>
        public static void RemoveGuidFromMap(this HashSet<Guid> map, IHaveGuid objectWithGuid)
        {
            if (map.Contains(objectWithGuid.Guid))
                map.Remove(objectWithGuid.Guid);
        }

        public static void RegenerateGuids(this TreeNode<IHaveGuid> tree)
        {
            var allNodes = tree.GetAllNodes().Select(n => n.value);
            allNodes.RegenerateGuids();
        }

        public static void RegenerateGuids(this IEnumerable<IHaveGuid> list)
        {
            foreach (var node in list)
            {
                if (node == null)
                    continue;

                node.RegenerateGuid();

                if (node is Recording s)
                    s.Commands.CastAndRemoveNullsTree<IHaveGuid>().RegenerateGuids();
            }
        }
    }
}
