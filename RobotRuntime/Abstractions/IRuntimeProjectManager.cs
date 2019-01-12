using System;

namespace RobotRuntime.Abstractions
{
    public interface IRuntimeProjectManager
    {
        string ProjectName { get; }
        void InitProject(string path);

        event Action<string> NewProjectOpened;
    }
}
