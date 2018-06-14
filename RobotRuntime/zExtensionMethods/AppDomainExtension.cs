using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RobotRuntime
{
    public static class AppDomainExtension
    {
        public static IEnumerable<Assembly> GetNativeAssemblies(this AppDomain AppDomain)
        {
            return AppDomain.GetAssemblies().Where(a => a.FullName.Contains("Robot"));
        }

        public static IEnumerable<Type> GetAllTypesWhichImplementInterface(this AppDomain AppDomain, Type interfaceType)
        {
            return AppDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }

        public static IEnumerable<Type> GetAllTypesWhichImplementInterface<T>(this T assemblies, Type interfaceType) where T : IEnumerable<Assembly>
        {
            return assemblies.SelectMany(s => s.GetTypes()).Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }

        public static IEnumerable<Type> GetAllTypesWhichImplementInterface(this Assembly assembly, Type interfaceType)
        {
            return assembly.GetTypes().Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }

        public static IEnumerable<string> GetAllAssembliesInBaseDirectory(this AppDomain appDomain)
        {
            foreach (var path in Directory.GetFiles(appDomain.BaseDirectory))
            {
                if (path.EndsWith(".dll") || path.EndsWith(".exe"))
                    yield return path;
            }
        }
    }
}
