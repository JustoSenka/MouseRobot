using MouseRobot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public static class BinaryObjectIO
    {
        public static T LoadScriptFile<T>(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T) binaryFormatter.Deserialize(stream);
            }
        }

        public static void SaveScriptFile<T>(string fileName, T objToWrite) 
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objToWrite);
            }
        }
    }
}
