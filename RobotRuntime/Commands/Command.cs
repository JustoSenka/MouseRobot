using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;
using System.Reflection;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : IRunnable, ICloneable, ISimilar
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public abstract string Name { get; }
        public abstract bool CanBeNested { get; }

        public abstract void Run(TestData TestData);
        public virtual object Clone()
        {
            var newInstance = Activator.CreateInstance(this.GetType());
            try
            {
                var fields = this.GetType().GetFields(k_BindingFlags);
                foreach (var field in fields)
                    field.SetValue(newInstance, field.GetValue(this));
            }
            catch (Exception)
            {
                Logger.Log(LogType.Error, "Command type throws exception when cloning. Cloning method must be incorrect: " + this.GetType());
            }
            return newInstance;
        }

        public bool Similar(object obj)
        {
            var c = obj as Command;
            if (c == null)
                return false;

            return this.ToString() == c.ToString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
