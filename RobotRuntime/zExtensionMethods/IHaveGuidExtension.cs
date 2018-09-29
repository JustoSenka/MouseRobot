using RobotRuntime.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime
{
    public static class IHaveGuidExtension
    {
        public static void AddGuidToMapAndGenerateUniqueIfNeeded(this HashSet<Guid> map, IHaveGuid objectWithGuid)
        {
            if (map.Contains(objectWithGuid.Guid))
            {
                Logger.Log(LogType.Warning, "Object '" + objectWithGuid.ToString() + "' has duplicate guid. Regenerating.");
                objectWithGuid.RegenerateGuid();
            }

            map.Add(objectWithGuid.Guid);
        }

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

                if (node is Script s)
                    s.Commands.CastAndRemoveNullsTree<IHaveGuid>().RegenerateGuids();
            }
        }
    }
}
