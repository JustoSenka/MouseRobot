namespace RobotRuntime.IO
{
    public abstract class ObjectIO
    {
        public abstract T LoadObject<T>(string path);
        public abstract void SaveObject<T>(string path, T objToWrite);

        public static ObjectIO Create()
        {
            // TODO: User prefs file/user settings or something

            return new YamlScriptIO();

            //if (false)
            return new BinaryObjectIO();
            /*else if (true)
                return new YamlObjectIO();
            else
                return null;*/
        }
    }
}
