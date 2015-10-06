using MouseRobot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace MouseRobot
{
    public static class BinaryObjectIO
    {
        public static T LoadObject<T>(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                return (T) new BinaryFormatter().Deserialize(stream);
            }
        }

        #warning "ASK: can it be, where T is [Serializable]"
        public static void SaveObject<T>(string fileName, T objToWrite)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                new BinaryFormatter().Serialize(stream, objToWrite);
            }
        }
    }
}
