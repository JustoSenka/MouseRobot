using System;
using System.Collections.Generic;

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
        /*
        public static void CheckGuidUniqueness(this HashSet<Guid> map, IEnumerable<IHaveGuid> objectWithGuid)
        {
            if (map.Contains(objectWithGuid.Guid))
                map.Remove(objectWithGuid.Guid);
        }*/
    }
}
