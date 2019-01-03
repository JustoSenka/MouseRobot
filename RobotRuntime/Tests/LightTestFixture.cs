﻿using RobotRuntime.Recordings;
using System;
using System.Collections.Generic;

namespace RobotRuntime.Tests
{
    public class LightTestFixture : ICloneable
    {
        public Recording Setup { get; set; }
        public Recording TearDown { get; set; }
        public Recording OneTimeSetup { get; set; }
        public Recording OneTimeTeardown { get; set; }

        [NonSerialized] public const string k_Setup = "Setup";
        [NonSerialized] public const string k_TearDown = "TearDown";
        [NonSerialized] public const string k_OneTimeSetup = "OneTimeSetup";
        [NonSerialized] public const string k_OneTimeTeardown = "OneTimeTeardown";

        public IList<Recording> Tests { get; set; }

        public Guid Guid { get; protected set; } = new Guid();

        public string Name { get; set; }

        public LightTestFixture(Guid guid = default(Guid))
        {
            Tests = new List<Recording>();
            Guid = guid == default(Guid) ? Guid.NewGuid() : guid;
        }

        public void AddScript(Recording s)
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

        public static bool IsSpecialScript(Recording Script)
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

            fixture.Setup = (Recording)Setup.Clone();
            fixture.TearDown = (Recording)TearDown.Clone();
            fixture.OneTimeSetup = (Recording)OneTimeSetup.Clone();
            fixture.OneTimeTeardown = (Recording)OneTimeTeardown.Clone();

            foreach (var test in Tests)
                fixture.Tests.Add((Recording)test.Clone());

            ((IHaveGuid)fixture.Setup).RegenerateGuid();
            ((IHaveGuid)fixture.TearDown).RegenerateGuid();
            ((IHaveGuid)fixture.OneTimeSetup).RegenerateGuid();
            ((IHaveGuid)fixture.OneTimeTeardown).RegenerateGuid();
            fixture.Tests.RegenerateGuids();

            fixture.Guid = Guid;

            return fixture;
        }
    }
}
