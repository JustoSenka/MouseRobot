using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using Unity;

namespace RobotRuntime
{
    public static class ObjectCreationExtension
    {
        public static IEnumerable<T> TryResolveTypes<T>(this IEnumerable<Type> types, IUnityContainer Container, ILogger Logger)
        {
            var list = new List<T>();

            foreach (var t in types)
            {
                try
                {
                    var instance = (T) Container.Resolve(t);
                    list.Add(instance);
                }
                catch (Exception e)
                {
                    Logger.Logi(LogType.Error, "Cannot resolve type: " + t.FullName, "Probably bad constructor. " + e.Message);
                }
            }

            return list;
        }
    }
}
