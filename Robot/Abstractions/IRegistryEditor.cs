namespace Robot.Abstractions
{
    public interface IRegistryEditor
    {
        object Get(string key);
        void Put(string key, object value);
    }
}
