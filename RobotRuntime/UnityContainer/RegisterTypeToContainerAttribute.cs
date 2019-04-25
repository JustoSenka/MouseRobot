using System;

namespace RobotRuntime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RegisterTypeToContainerAttribute : Attribute
    {
        public Type InterfaceType { get; }
        public Type LifetimeManagerType { get; }

        /// <summary>
        /// lifetimeManagerType should inherit from LifetimeManager type
        /// </summary>
        public RegisterTypeToContainerAttribute(Type interfaceType, Type lifetimeManagerType = null) : base()
        {
            this.InterfaceType = interfaceType;
            this.LifetimeManagerType = lifetimeManagerType;
        }
    }
}
