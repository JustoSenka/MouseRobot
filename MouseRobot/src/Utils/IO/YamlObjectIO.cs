using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Robot.IO
{
    public class YamlObjectIO : ObjectIO
    {
        public override T LoadObject<T>(string path)
        {
            try
            {
                var text = File.ReadAllText(path);
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<T>(text);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to read or deserialize file: " + path);
            }
            return default(T);
        }

        public override void SaveObject<T>(string path, T objToWrite)
        {
            try
            {
                var serializer = new SerializerBuilder().Build();
                var text = serializer.Serialize(objToWrite);
                File.WriteAllText(path, text);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to serialize and write file: " + path);
            }
        }
    }
}
