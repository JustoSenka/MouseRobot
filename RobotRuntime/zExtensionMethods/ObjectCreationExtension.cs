using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using Unity;

namespace RobotRuntime
{
    public static class ObjectCreationExtension
    {
        public static IEnumerable<object> TryResolveTypes<T>(this T types, IUnityContainer Container, ILogger Logger) where T : IEnumerable<Type>
        {
            var list = new List<object>();

            foreach (var t in types)
            {
                try
                {
                    var instance = Container.Resolve(t);
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
