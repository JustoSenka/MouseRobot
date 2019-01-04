using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Base
{
    public static class TestData
    {
        public static string TempProjectPath
        {
            get
            {
                return Path.GetTempPath() + "\\MProject";
            }
        }

        public static LightTestFixture LightTestFixture
        {
            get
            {
                var f = new LightTestFixture();
                f.Name = "TestName";
                f.Setup = new Recording();
                f.TearDown = new Recording();
                f.OneTimeSetup = new Recording();
                f.OneTimeTeardown = new Recording();
                f.Setup.Name = LightTestFixture.k_Setup;
                f.TearDown.Name = LightTestFixture.k_TearDown;
                f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
                f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
                f.Tests = new Recording[] { new Recording(), new Recording() }.ToList();
                return f;
            }
        }

        public static Recording NewRecording(out Command topCommand, out Command childCommand)
        {
            var s = new Recording();
            topCommand = s.AddCommand(new CommandSleep(1));
            childCommand = s.AddCommand(new CommandSleep(2), topCommand);
            return s;
        }
    }
}
