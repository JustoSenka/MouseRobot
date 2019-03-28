using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace RobotRuntime
{
    public class PluginImporter : AssetImporter
    {
        public PluginImporter(string path) : base(path) { }

        protected override object LoadAsset()
        {
            var bytes = File.ReadAllBytes(Path);
            return Assembly.Load(bytes);
        }

        public override void SaveAsset()
        {
            if (Value is Assembly a)
            {
                using (FileStream stream = new FileStream(Path, FileMode.OpenOrCreate))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, a);
                }
            }
            else if (Value is byte[] bytes)
            {
                File.WriteAllBytes(Path, bytes);
            }
        }

        public override Type HoldsType()
        {
            return typeof(Assembly);
        }
    }
}
