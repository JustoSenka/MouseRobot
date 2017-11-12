using System;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : ICloneable
    {
        public abstract void Run();
        public abstract object Clone();
    }

    public enum CommandType
    {
        Down, Move, Press, Release, Sleep, MoveOnImage
    }
}
