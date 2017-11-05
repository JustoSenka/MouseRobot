using System.Collections.Generic;

namespace RobotRuntime.Perf
{
    public class ProfilerNode
    {
        public string Name { get; private set; }
        public int Time { get; private set; }
        public List<ProfilerNode> Children { get; private set; }

        public ProfilerNode(string name, int time)
        {
            this.Name = name;
            this.Time = time;
            this.Children = new List<ProfilerNode>();
        }
    }
}
