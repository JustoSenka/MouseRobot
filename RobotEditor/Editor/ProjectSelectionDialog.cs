using Robot;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using RobotEditor.Abstractions;

namespace RobotEditor.Editor
{
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
            if (CommonFileDialog.IsPlatformSupported)
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Title = k_Title;
                dialog.AddToMostRecentlyUsedList = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    ProjectManager.InitProject(dialog.FileName);
                    return true;
                }
            }
            else
            {
                var dialog = new FolderBrowserDialog();
                dialog.Description = k_Title;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ProjectManager.InitProject(dialog.SelectedPath);
                    return true;
                }
            }

            return false;
        }
    }
}

