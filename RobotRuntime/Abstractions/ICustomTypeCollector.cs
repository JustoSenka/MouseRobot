using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface ICustomTypeCollector<T>
    {
        IEnumerable<Type> AllTypes { get; }
        IEnumerable<Type> UserTypes { get; }

        bool IsNative(Type type);
        event Action NewTypesAppeared;
    }
}
