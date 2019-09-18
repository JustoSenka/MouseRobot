using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Robot
{
    public interface IProjectManager : IRuntimeProjectManager
    {
        IList<string> LastKnownProjectPaths { get; }

        Task RestoreAndRemovePathsOfDeletedProjects();

        bool IsPathAProject(string path);

        Task RestoreSettings();
        Task SaveSettings();
        Task RememberPathInSettings(string path);

    }
}