using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface ICustomTypeObjectCollector<T> : ICustomTypeCollector<T>
    {
        void RestoreDefaultObjects();

        IEnumerable<T> AllObjects { get; }
        Dictionary<Type, T> TypeObjectMap { get; }
    }
}