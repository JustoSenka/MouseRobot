﻿using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface ITypeObjectCollector<T> : ITypeCollector<T>
    {
        void RestoreDefaultObjects();

        IEnumerable<T> AllObjects { get; }
        IEnumerable<T> UserObjects { get; }
        Dictionary<Type, T> TypeObjectMap { get; }
    }
}