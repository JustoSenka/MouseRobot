using RobotRuntime.IO;
using System;

namespace RobotRuntime
{
    public abstract partial class AssetImporter
    {
        protected class LightScriptImporter : AssetImporter
        {
            public LightScriptImporter(string path) : base(path) { }

            protected override object LoadAsset()
            {
                return ObjectIO.Create().LoadObject<LightScript>(Path);
            }

            public override void SaveAsset()
            {
                ObjectIO.Create().SaveObject(Path, (LightScript)Value);
            }

            public override Type HoldsType()
            {
                return typeof(LightScript);
            }
        }
    }
}
