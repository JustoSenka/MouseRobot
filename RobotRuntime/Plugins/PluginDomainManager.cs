using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace RobotRuntime.Plugins
{
    public class PluginDomainManager : MarshalByRefObject
    {
        public AppDomain AppDomain;
        public Assembly[] Assemblies;

        public PluginDomainManager()
        {

        }

        public T ResolveInterface<T>(UnityContainer Container)
        {
            return Container.Resolve<T>();
        }

        public object Instantiate(string className)
        {
            var type = Assemblies.SelectMany(a => a.GetTypes()).First(t => t.FullName == className);
            return Activator.CreateInstance(type);
        }
    }
}
