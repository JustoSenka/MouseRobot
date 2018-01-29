using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime
{
    public static class AppDomainExtension
    {
        public static IEnumerable<Type> GetAllTypesWhichImplementInterface(this AppDomain AppDomain, Type interfaceType)
        {
            return AppDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(
                p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }
    }
}
