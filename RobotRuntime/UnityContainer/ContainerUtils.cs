using System;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace RobotRuntime
{
    public class ContainerUtils
    {
        /// <summary>
        /// Will fetch all types which have RegisterTypeToContainerAttribute attribute attached to them
        /// and registered those types to container based on attribute values
        /// </summary>
        public static void Register(IUnityContainer Container, Assembly assembly)
        {
            var allTypes = assembly.GetTypes();
            var allTypesWithAttribute = allTypes.Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(RegisterTypeToContainerAttribute)));
            var typeAttributeMap = allTypesWithAttribute
                .SelectMany(t => t.GetCustomAttributes(typeof(RegisterTypeToContainerAttribute))
                .Select(a => (Type: t, Attribute: (RegisterTypeToContainerAttribute) a)));

            var typeMap = typeAttributeMap.Select(t => (t.Type, t.Attribute.InterfaceType, t.Attribute.LifetimeManagerType));

            foreach (var (Type, InterfaceType, LifetimeManagerType) in typeMap)
            {
                var lifetimeManager = LifetimeManagerType != null ? Activator.CreateInstance(LifetimeManagerType) as LifetimeManager : null;
                if (lifetimeManager == null)
                    Container.RegisterType(InterfaceType, Type);
                else
                    Container.RegisterType(InterfaceType, Type, lifetimeManager);
            }
        }

        /// <summary>
        /// It is here so resolving will not fail because of integers, guids or booleans in constructors. Not really used currently.
        /// In perfect scenarion, Unity should not try to resolve class which has primitives in constructors.
        /// </summary>
        public static void RegisterPrimitiveTypes(IUnityContainer Container)
        {
            var allPrimitiveTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsPrimitive);
            foreach (var t in allPrimitiveTypes)
                Container.RegisterType(t, new InjectionFactory((c) => default));

            Container.RegisterType<Guid>(new InjectionConstructor(Guid.Empty.ToString()));
            Container.RegisterType<DateTime>(new InjectionConstructor((long)0));
        }
    }
}
