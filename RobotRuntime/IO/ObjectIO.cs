namespace RobotRuntime.IO
{
    public abstract class ObjectIO
    {
        public abstract T LoadObject<T>(string path);
        public abstract void SaveObject<T>(string path, T objToWrite);

        // TODO: no references, will need to rethink design on this one since separate things use separate serializers
        public static ObjectIO Create()
        {
            // TODO: User prefs file/user settings or something

            //return new YamlScriptIO();

            //if (false)
            return new BinaryObjectIO();
            /*else if (true)
                return new YamlObjectIO();
            else
                return null;*/
        }
    }
}
