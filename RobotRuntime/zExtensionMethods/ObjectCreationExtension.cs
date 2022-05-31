using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using Unity;

namespace RobotRuntime
{
    public static class ObjectCreationExtension
    {

        /// <summary>
        /// Tries to instantiate all types using Container or Activator, if both fail, type is skipped.
        /// Creates new list with created instances and returns it.
        /// T - Is base type or interface of Instances to be created.
        /// exactType - is exact type of instance to be created.
        /// </summary>
        public static IEnumerable<T> TryResolveTypes<T>(this IEnumerable<Type> exactTypes, IUnityContainer Container, ILogger Logger)
        {
            var list = new List<T>();

            foreach (var t in exactTypes)
            {
                var instance = Container.TryResolve<T>(t);

                if (!instance.IsDefault())
                    list.Add(instance);
            }

            return list;
        }

        public static T TryResolve<T>(this IUnityContainer Container) => Container.TryResolve<T>(typeof(T));

        /// <summary>
        /// Tries to instantiate type using Container or Activator. If both fail, default is returned.
        /// typeToCreate - Type to be created.
        /// </summary>
        public static T TryResolve<T>(this IUnityContainer Container, Type typeToCreate)
        {
            T instance = default;
            Exception ex = null;
            try
            {
                instance = (T)Container.Resolve(typeToCreate);
            }
            catch (Exception e)
            {
                ex = e;
                try
                {
                    instance = (T)Activator.CreateInstance(typeToCreate); // if UnityContainer cannot create object, try default ctor
                }
                catch { } // Probably doesn't have one
            }

            if (!instance.IsDefault())
                return instance;
            else
            {
                Logger.Log(LogType.Error, "Cannot resolve type: " + typeToCreate.FullName, "Make sure ctor is correct and/or empty ctor exists. " + ex.Message);
                return default;
            }
        }
    }
}
