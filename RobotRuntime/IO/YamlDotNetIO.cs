using System;
using System.IO;
using YamlDotNet.Serialization;

namespace RobotRuntime.IO
{
    public class YamlDotNetIO : ObjectIO
    {
        public override T LoadObject<T>(string path)
        {
            try
            {
                var text = File.ReadAllText(path);
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<T>(text);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to read from file: " + path, e.Message);
            }
            return default(T);
        }

        public override void SaveObject<T>(string path, T objToWrite)
        { 
            try
            {
                var serializer = new SerializerBuilder().EnsureRoundtrip().EmitDefaults().Build();
                var text = serializer.Serialize(objToWrite);
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to write to file: " + path, e.Message);
            }
        }
    }
}
