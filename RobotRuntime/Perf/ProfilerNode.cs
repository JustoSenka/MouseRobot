using System.Collections.Generic;

namespace RobotRuntime.Perf
{
    public class ProfilerNode
    {
        public string Name { get; private set; }
        public string Time { get; private set; }
        public List<ProfilerNode> Children { get; private set; }

        public ProfilerNode(string name, string time)
        {
            this.Name = name;
            this.Time = time;
            this.Children = new List<ProfilerNode>();
        }
    }
}
