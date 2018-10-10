using System;
using System.IO;
using System.Reflection;

namespace RobotRuntime
{
    public abstract partial class AssetImporter
    {
        protected class ExtensionImporter : AssetImporter
        {
            public ExtensionImporter(string path) : base(path) { }

            protected override object LoadAsset()
            {
                var bytes = File.ReadAllBytes(Path);
                return Assembly.Load(bytes);
            }

            public override void SaveAsset()
            {
                Logger.Log(LogType.Error, "Saving DLL file is not supported.");
            }

            public override Type HoldsType()
            {
                return typeof(Assembly);
            }
        }
    }
}
