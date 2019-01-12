﻿using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace RobotRuntime.Scripts
{
    public class CustomTypeObjectCollector<T> : CustomTypeCollector<T>, ICustomTypeObjectCollector<T>, ICustomTypeCollector<T>
    {
        public IEnumerable<T> AllObjects { get { return m_AllObjects; } }
        public Dictionary<Type, T> TypeObjectMap { get; private set; }

        private T[] m_NativeObjects;
        private T[] m_UserObjects;
        private T[] m_AllObjects;

        private readonly ILogger Logger;
        private readonly IUnityContainer Container;
        public CustomTypeObjectCollector(IScriptLoader ScriptLoader, ILogger Logger, IUnityContainer Container) : base(ScriptLoader)
        {
            this.Logger = Logger;
            this.Container = Container;

            CollectNativeTypes();
        }

        public void RestoreDefaultObjects()
        {
            CollectNativeTypes();
            CollectUserTypes();
        }

        protected override void CollectNativeTypes()
        {
            base.CollectNativeTypes();

            if (Container != null)
            {
                m_NativeObjects = base.AllTypes.TryResolveTypes<T>(Container, Logger).ToArray();

                m_AllObjects = m_NativeObjects;
                BuildTypeObjectMap();
            }
        }

        protected override void CollectUserTypes()
        {
            base.CollectUserTypes();

            if (Container != null)
            {
                m_UserObjects = base.UserTypes.TryResolveTypes<T>(Container, Logger).ToArray();

                m_AllObjects = m_NativeObjects.Concat(m_UserObjects).ToArray();
                BuildTypeObjectMap();
            }
        }

        private void BuildTypeObjectMap()
        {
            TypeObjectMap = new Dictionary<Type, T>();
            foreach (var obj in AllObjects)
                TypeObjectMap.Add(obj.GetType(), obj);
        }
    }
}
