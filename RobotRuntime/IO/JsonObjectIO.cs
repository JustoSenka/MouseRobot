using Newtonsoft.Json;
using System.IO;

namespace RobotRuntime.IO
{
    public class JsonObjectIO : ObjectIO
    {
        public override T LoadObject<T>(string fileName)
        {
            try
            {
                var json = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException e)
            {
                Logger.Log(LogType.Error, "Failed to deserialize object from json: " + fileName, e.Message);
                return default;
            }
            catch (IOException e)
            {
                Logger.Log(LogType.Error, "Failed to read from file: " + fileName, e.Message);
                return default;
            }
        }

        public override void SaveObject<T>(string fileName, T objToWrite)
        {
            try
            {
                var json = JsonConvert.SerializeObject(objToWrite, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
            catch (JsonException e)
            {
                Logger.Log(LogType.Error, "Failed to serialize object to json: " + fileName, e.Message);
            }
            catch (IOException e)
            {
                Logger.Log(LogType.Error, "Failed to write to file: " + fileName, e.Message);
            }
        }
    }
}
