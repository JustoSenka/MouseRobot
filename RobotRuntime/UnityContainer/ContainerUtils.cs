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
        public static BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

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
        /// Will fetch all types which have RegisterTypeToContainerAttribute attribute attached to them
        /// and registered those types to container based on attribute values
        /// It is used to pass dependencies to UITypeEditor and StringConverter of .net classes. Since I cannot control the instantiation of these classes,
        /// no other way to pass dependencies. Now it is done in a hacky way. Passed through reflection to private static field.
        /// </summary>
        public static void PassStaticDependencies(IUnityContainer Container, Assembly assembly)
        {
            var allTypes = assembly.GetTypes();
            var allPropsWithAttribute = allTypes.SelectMany(t => t.GetProperties(StaticNonPublic))
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(RequestStaticDependencyAttribute))); 
            var allFieldsWithAttribute = allTypes.SelectMany(t => t.GetFields(StaticNonPublic))
                .Where(f => f.CustomAttributes.Any(a => a.AttributeType == typeof(RequestStaticDependencyAttribute)));

            var propAttributeMap = allPropsWithAttribute
                .SelectMany(p => p.GetCustomAttributes(typeof(RequestStaticDependencyAttribute))
                .Select(a => (Prop: p, Attribute: (RequestStaticDependencyAttribute)a)));

            var fieldAttributeMap = allFieldsWithAttribute
                .SelectMany(f => f.GetCustomAttributes(typeof(RequestStaticDependencyAttribute))
                .Select(a => (Field: f, Attribute: (RequestStaticDependencyAttribute)a)));

            foreach (var (Prop, Attribute) in propAttributeMap)
            {
                var dependency = Container.TryResolve<object>(Attribute.DependencyType);
                Prop.SetValue(Prop, dependency);
            }

            foreach (var (Field, Attribute) in fieldAttributeMap)
            {
                var dependency = Container.TryResolve<object>(Attribute.DependencyType);
                Field.SetValue(Field, dependency);
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
