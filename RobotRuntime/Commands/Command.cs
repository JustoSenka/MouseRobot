using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;
using System.Reflection;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : IRunnable, ICloneable, ISimilar, IHaveGuid
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public Guid Guid { get; protected set; } = Guid.NewGuid();

        public abstract string Name { get; }
        public abstract bool CanBeNested { get; }

        public abstract void Run(TestData TestData);

        public Command(Guid guid = default(Guid))
        {
            Guid = guid == default(Guid) ? Guid.NewGuid() : guid;
        }

        public virtual object Clone()
        {
            object newInstance = null;
            try
            {
                newInstance = Activator.CreateInstance(this.GetType());

                var fields = this.GetType().GetFields(k_BindingFlags);
                foreach (var field in fields)
                    field.SetValue(newInstance, field.GetValue(this));
            }
            catch (Exception)
            {
                Logger.Log(LogType.Error, "Command throws exception when cloning. Cloning method must be incorrect: " + this.GetType());
            }
            return newInstance;
        }

        public bool Similar(object obj)
        {
            if (!(obj is Command c))
                return false;

            return this.ToString() == c.ToString();
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Implemented explicitly so it has less visibility, since most systems should not regenerate guids by themself.
        /// As of this time, only scripts need to regenerate guids for commands (2018.08.15)
        /// </summary>
        void IHaveGuid.RegenerateGuid()
        {
            Guid = Guid.NewGuid();
        }
    }
}
