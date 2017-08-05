using System;
using System.Drawing;
using System.Windows.Forms;
using Robot;
using System.Diagnostics;
using RobotUI.Utils;
using Robot.Utils.Win32;

namespace RobotUI
{
    public partial class TreeViewForm : Form
    {
        bool keyDown = false;
        decimal timeDown = 0;

        public TreeViewForm()
        {
            InitializeComponent();

            this.Font = Fonts.Default;
            treeView.Font = Fonts.Default;
            //this.Controls.ForEach((Control c) => c.Font = Fonts.Default);

            ScriptTreeViewUtils.UpdateTreeView(treeView);
            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            //this.Close();
            this.Dispose();
            GC.Collect();
            Application.Exit();
        }

        #region Command registering functionality
        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyDown)
            {
                return;
            }
            keyDown = true;
            timeDown = (decimal)DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;

            int x = WinAPI.GetCursorPosition().X, y = WinAPI.GetCursorPosition().Y;

            switch (e.KeyData.ToString().ToUpper())
            {
                case "S":
                    Console.WriteLine("Press: (" + x + " ," + y + ")");
                    break;
                case "D":
                    Console.WriteLine("Press: (" + x + " ," + y + ") and sleep");
                    break;
                case "F":
                    Console.WriteLine("Sleep for...");
                    break;
                case "G":
                    Console.WriteLine("Default sleep for " + textBox2.Text);
                    break;
                case "H":
                    Console.WriteLine("Down on: (" + x + " ," + y + ")");
                    break;
                case "J":
                    Console.WriteLine("Move to: (" + x + " ," + y + ")");
                    break;
                case "K":
                    Console.WriteLine("Release");
                    break;
                case "R":
                    Console.WriteLine("Run Script");
                    break;
                case "Q":
                    Console.WriteLine("Empty script");
                    break;
            }

            ScriptTreeViewUtils.UpdateTreeView(treeView);
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {

            int x = WinAPI.GetCursorPosition().X, y = WinAPI.GetCursorPosition().Y;
            timeDown = (decimal)DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond - timeDown;

            switch (e.KeyData.ToString().ToUpper())
            {
                case "S":
                    MouseRobot.Instance.AddCommandPress(x, y);
                    break;
                case "D":
                    MouseRobot.Instance.AddCommandPress(x, y);
                    MouseRobot.Instance.AddCommandSleep((int)timeDown);
                    Console.WriteLine("Sleep for..." + timeDown);
                    break;
                case "F":
                    MouseRobot.Instance.AddCommandSleep((int)timeDown);
                    Console.WriteLine("Sleep for..." + timeDown);
                    break;
                case "G":
                    int sleepTime;
                    try
                    {
                        sleepTime = Int32.Parse(textBox2.Text);
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("\"" + textBox2.Text + "\" is not a valid number.");
                        Console.WriteLine(fe.ToString());
                        sleepTime = 1000;
                    }
                    MouseRobot.Instance.AddCommandSleep(sleepTime);
                    break;
                case "H":
                    MouseRobot.Instance.AddCommandDown(x, y);
                    break;
                case "J":
                    MouseRobot.Instance.AddCommandMove(x, y);
                    break;
                case "K":
                    MouseRobot.Instance.AddCommandRelease();
                    break;
                case "R":

                    int repeatTimes = TryReadRepeatTimes();
                    TryStartScript(repeatTimes);

                    break;
            }

            keyDown = false;
            ScriptTreeViewUtils.UpdateTreeView(treeView);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MouseRobot.Instance.StopScript();
        }
        #endregion

        #region DEPRECATED Try read/try start script

        private int TryReadRepeatTimes()
        {
            int repeatTimes;
            try
            {
                repeatTimes = Int32.Parse(textBox1.Text);
            }
            catch (FormatException fe)
            {
                Console.WriteLine("\"" + textBox1.Text + "\" is not a valid number." + fe.StackTrace);
                repeatTimes = 1;
            }
            return repeatTimes;
        }

        private void TryStartScript(int repeatTimes)
        {
            try
            {
                MouseRobot.Instance.StartScript(repeatTimes);
            }
            catch (EmptyScriptException ese)
            {
                Console.WriteLine(ese.Message);
            }
        }

        #endregion




        #region TreeView Drag and Drop
        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.All);
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            ScriptTreeViewUtils.TreeView_DragDrop(treeView, e);
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            ScriptTreeViewUtils.TreeView_DragOver(treeView, e);
        }
        #endregion


        #region Menu Items (ScriptManager)
        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.OpenScript(treeView);
        }

        private void saveAllScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveAllScripts(treeView);
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveScript(ScriptManager.Instance.ActiveScript, treeView, true);
        }

        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SetSelectedScriptActive(treeView);
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.NewScript(treeView);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(treeView);
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(treeView);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(treeView);
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(treeView);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.ShowSelectedTreeViewItemInExplorer(treeView);
        }
        #endregion

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var window = new TreeViewWindow();

        }
    }
}
