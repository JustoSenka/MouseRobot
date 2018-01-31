using System.Collections.Generic;

namespace Robot
{
    public interface IProjectManager
    {
        IList<string> LastKnownProjectPaths { get; }

        void RestoreAndRemovePathsOfDeletedProjects();

        void InitProject(string path);
        bool IsPathAProject(string path);

        void RestoreSettings();
        void SaveSettings();
        void RememberPathInSettings(string path);
    }
}