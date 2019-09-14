using System;
using System.Threading.Tasks;

namespace RobotRuntime.Abstractions
{
    public interface IRuntimeProjectManager
    {
        string ProjectName { get; }

        /// <summary>
        /// Will initialize new project asynchronously.
        /// Creates all managers, updates Environment.CurrentDirectory.
        /// Refreshes assets and compiles scripts
        /// </summary>
        Task InitProject(string path);

        /// <summary>
        /// Callback is fired when asset scan is complete, scripts are compiled and loaded.
        /// It might be called from different thread in asynchronous context.
        /// </summary>
        event Action<string> NewProjectOpened;
    }
}
