using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RobotRuntime
{
    public static class AppDomainExtension
    {
        public static IEnumerable<Type> GetAllTypesWhichImplementInterface(this AppDomain AppDomain, Type interfaceType)
        {
            return AppDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }

        public static IEnumerable<Type> GetAllTypesWhichImplementInterface(this Assembly[] assemblies, Type interfaceType)
        {
            return assemblies.SelectMany(s => s.GetTypes()).Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }

        public static IEnumerable<Type> GetAllTypesWhichImplementInterface(this Assembly assembly, Type interfaceType)
        {
            return assembly.GetTypes().Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }
    }
}
