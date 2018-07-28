using RobotRuntime.Scripts;
using System;
using System.Collections.Generic;

namespace RobotRuntime.Tests
{
    public class LightTestFixture : ICloneable
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

        public static bool IsSpecialScript(Script Script)
        {
            return (Script.Name == k_OneTimeSetup ||
                Script.Name == k_OneTimeTeardown ||
                Script.Name == k_Setup ||
                Script.Name == k_TearDown);
        }

        public object Clone()
        {
            var fixture = new LightTestFixture();
            fixture.Name = Name;

            fixture.Setup = (Script)Setup.Clone();
            fixture.TearDown = (Script)TearDown.Clone();
            fixture.OneTimeSetup = (Script)OneTimeSetup.Clone();
            fixture.OneTimeTeardown = (Script)OneTimeTeardown.Clone();

            foreach (var test in Tests)
                fixture.Tests.Add((Script)test.Clone());

            return fixture;
        }
    }
}
