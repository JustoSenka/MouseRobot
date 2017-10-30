using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace RobotRuntime.IO
{
    public class BinaryObjectIO : ObjectIO
    {
        public override T LoadObject<T>(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                try
                {
                    return (T)new BinaryFormatter().Deserialize(stream);
                }
                catch (SerializationException)
                {
                    Console.WriteLine("Failed to read from file: " + fileName);
                    return default(T);
                }
            }
        }

        public override void SaveObject<T>(string fileName, T objToWrite)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                try
                {
                    new BinaryFormatter().Serialize(stream, objToWrite);
                }   
                catch(SerializationException)
                {
                    Console.WriteLine("Failed to write to file: " + fileName);
                }
            }
        }
    }
}
