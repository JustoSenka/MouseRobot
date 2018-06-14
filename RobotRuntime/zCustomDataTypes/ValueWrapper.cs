using System;

namespace RobotRuntime
{
    [Serializable]
    public class ValueWrapper<T> where T : struct
    {
        public T Value { get; set; }
        public ValueWrapper(T value) { this.Value = value; }
    }

    [Serializable]
    public class ValueWrapper : MarshalByRefObject
    {
        public object Value { get; set; }
        public ValueWrapper(object value) { this.Value = value; }
    }
}
