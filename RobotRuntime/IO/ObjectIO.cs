namespace RobotRuntime.IO
{
    public abstract class ObjectIO
    {
        public abstract T LoadObject<T>(string path);
        public abstract void SaveObject<T>(string path, T objToWrite);

        public static ObjectIO Create()
        {
            // TODO: User prefs file/user settings or something

            //if (true)
                return new BinaryObjectIO();
            /*else if (false)
                return new YamlObjectIO();
            else
                return null;*/
        }
    }
}
