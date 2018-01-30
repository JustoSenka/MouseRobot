namespace Robot
{
    public interface IProjectManager
    {
        void InitProject(string path);
        bool IsPathAProject(string path);
    }
}