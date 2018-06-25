using RobotRuntime.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotRuntime.Tests
{
    public class LightTestFixture
    {
        public Script Setup { get; set; }
        public Script TearDown { get; set; }
        public Script OneTimeSetup { get; set; }
        public Script OneTimeTeardown { get; set; }

        [NonSerialized] public const string k_Setup = "Setup";
        [NonSerialized] public const string k_TearDown = "TearDown";
        [NonSerialized] public const string k_OneTimeSetup = "OneTimeSetup";
        [NonSerialized] public const string k_OneTimeTeardown = "OneTimeTeardown";

        public IList<Script> Tests { get; set; }

        public string Name { get; set; }

        public LightTestFixture()
        {
            Tests = new List<Script>();
        }

        public void AddScript(Script s)
        {
            if (s.Name == k_Setup)
                Setup = s;
            else if (s.Name == k_TearDown)
                TearDown = s;
            else if (s.Name == k_OneTimeSetup)
                OneTimeSetup = s;
            else if (s.Name == k_OneTimeTeardown)
                OneTimeTeardown = s;
            else
                Tests.Add(s);
        }
    }
}
