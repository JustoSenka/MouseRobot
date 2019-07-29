using Microsoft.WindowsAPICodePack.Dialogs;
using Robot;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Utils;
using System.Windows.Forms;
using Unity.Lifetime;

namespace RobotEditor.Editor
{
    [RegisterTypeToContainer(typeof(IProjectSelectionDialog), typeof(ContainerControlledLifetimeManager))]
    public class ProjectSelectionDialog : IProjectSelectionDialog
    {
        private const string k_Title = "Select Project Directory";

        private IProjectManager ProjectManager;
        public ProjectSelectionDialog(IProjectManager ProjectManager)
        {
            this.ProjectManager = ProjectManager;
        }

        public bool InitProjectWithDialog()
        {
            var selected = ShowDialogToSelectFolder(out string selectedPath);
            if (selected)
                ProjectManager.InitProject(selectedPath);

            return selected;
        }

        public bool OpenNewProgramInstanceOfProjectWithDialog()
        {
            var selected = ShowDialogToSelectFolder(out string selectedPath);
            if (selected)
                ProcessUtility.StartFromCommandLine(Paths.ApplicationExecutablePath,  selectedPath.Quated(), false);

            return selected;
        }

        private bool ShowDialogToSelectFolder(out string selectedPath)
        {
            if (CommonFileDialog.IsPlatformSupported)
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Title = k_Title;
                dialog.AddToMostRecentlyUsedList = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    selectedPath = dialog.FileName;
                    return true;
                }
            }
            else
            {
                var dialog = new FolderBrowserDialog();
                dialog.Description = k_Title;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = dialog.SelectedPath;
                    return true;
                }
            }

            selectedPath = "";
            return false;
        }
    }
}

