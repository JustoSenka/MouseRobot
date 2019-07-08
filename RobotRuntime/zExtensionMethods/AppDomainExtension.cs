using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RobotRuntime
{
    public static class AppDomainExtension
    {
        public static IEnumerable<Assembly> GetNativeAssemblies(this AppDomain AppDomain)
        {
            return AppDomain.GetAssemblies().Where(a => Regex.IsMatch(a.ManifestModule.ScopeName,
                @"^Robot\.(dll|exe)$|^RobotEditor\.(dll|exe)$|^RobotRuntime\.(dll|exe)$|^MouseRobot\.(dll|exe)$", RegexOptions.IgnoreCase));
        }

        // Really bad hack, needed to get user assemblies without getting all trashed assemblies from previous compilations
        // This field is set Before initialization using reflection to get all attributes
        [RequestStaticDependency(typeof(IScriptLoader))]
        private static IScriptLoader ScriptLoader { get; set; }

        [Obsolete("This method will lead to more mistakes than goods. Be aware that AppDomain will contain out of date" +
            " user assemblies. Better use GetNativeAssemblies or GetUserAssemblies from ScriptLoader")]
        public static IEnumerable<Assembly> GetAllAssemblies(this AppDomain AppDomain)
        {
            if (ScriptLoader != null)
            {
                var userAssemblies = ScriptLoader.IterateUserAssemblies(a => a);
                var nativeAssemblies = AppDomain.GetNativeAssemblies();
                return nativeAssemblies.Concat(userAssemblies);
            }
            else
                return AppDomain.GetAssemblies();
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
                if (path.EndsWith(FileExtensions.ExeD) || path.EndsWith(FileExtensions.DllD))
                    yield return path;
            }
        }
    }
}
