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
                T instance = default;
                Exception ex = default;

                try
                {
                    instance = (T) Container.Resolve(t);
                }
                catch (Exception e)
                {
                    ex = e;
                    try
                    {
                        instance = (T) Activator.CreateInstance(t); // if UnityContainer cannot create object, try default ctor
                    }
                    catch { } // Probably doesn't have one
                }

                if (instance != default)
                    list.Add(instance);
                else
                    Logger.Logi(LogType.Error, "Cannot resolve type: " + t.FullName, "Make sure ctor is correct and/or empty ctor exists. " + ex.Message);
            }

            return list;
        }
    }
}
