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

        public IList<Script> Tests { get; private set; }

        public LightTestFixture()
        {

        }
    }
}
