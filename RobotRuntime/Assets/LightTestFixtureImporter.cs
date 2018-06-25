using RobotRuntime.IO;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime
{
    public abstract partial class AssetImporter
    {
        protected class LightTestFixtureImporter : AssetImporter
        {
            public LightTestFixtureImporter(string path) : base(path) { }

            protected override object LoadAsset()
            {
                return new YamlTestFixtureIO().LoadObject<LightTestFixture>(Path);
            }

            public override void SaveAsset()
            {
                new YamlTestFixtureIO().SaveObject(Path, ((LightTestFixture)Value));
            }

            public override Type HoldsType()
            {
                return typeof(LightTestFixture);
            }
        }
    }
}
