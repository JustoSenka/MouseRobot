using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;

namespace Robot
{
    public interface IProjectManager : IRuntimeProjectManager
    {
        IList<string> LastKnownProjectPaths { get; }

        void RestoreAndRemovePathsOfDeletedProjects();

        bool IsPathAProject(string path);

        void RestoreSettings();
        void SaveSettings();
        void RememberPathInSettings(string path);

    }
}