using Robot;
using RobotRuntime;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotEditor.Editor
{
    public class ProjectSelectionDialog
    {
        public ProjectSelectionDialog()
        {
            var openDialog = new OpenFileDialog();
            openDialog.InitialDirectory = Environment.CurrentDirectory + "\\" + Paths.ScriptFolder;
            openDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Script);
            openDialog.Title = "Select a path for script to save.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
            }
        }
    }
}
