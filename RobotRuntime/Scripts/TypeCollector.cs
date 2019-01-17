using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime.Scripts
{
    public class TypeCollector<T> : ITypeCollector<T>
    {
        public IEnumerable<Type> AllTypes { get { return m_AllTypes; } }
        public IEnumerable<Type> UserTypes { get { return m_UserTypes; } }

        public event Action NewTypesAppeared;

        private Type[] m_NativeTypes;
        private Type[] m_UserTypes;
        private Type[] m_AllTypes;

        private readonly IScriptLoader ScriptLoader;
        public TypeCollector(IScriptLoader ScriptLoader)
        {
            this.ScriptLoader = ScriptLoader;

            ScriptLoader.UserDomainReloaded += OnDomainReloaded;

            // Callback from the first call will practically do nothing
            // Since this object is created before the system using it
            // System will subscribe to TypeCollector only after TypeCollector finishes its constructor
            CollectNativeTypes();
            CollectUserTypes();
        }

        private void OnDomainReloaded()
        {
            CollectUserTypes();
            NewTypesAppeared?.Invoke();
        }

        protected virtual void CollectNativeTypes()
        {
            m_NativeTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(T)).ToArray();
            m_AllTypes = m_NativeTypes;
        }

        protected virtual void CollectUserTypes()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain

            m_UserTypes = ScriptLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(T)).ToArray();
            m_AllTypes = m_NativeTypes.Concat(m_UserTypes).ToArray();
        }

        public bool IsNative(Type type)
        {
            return (m_NativeTypes != null) ? m_NativeTypes.Contains(type) : false;
        }
    }
}
