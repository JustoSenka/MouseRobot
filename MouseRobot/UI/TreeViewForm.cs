#define ENABLE_UI_TESTING

using System;
using System.Drawing;
using System.Windows.Forms;
using Robot;
using System.Diagnostics;

namespace RobotUI
{
    public partial class TreeViewForm : Form
    {
        bool keyDown = false;
        decimal timeDown = 0;

        public TreeViewForm()
        {
            InitializeComponent();
            UpdateTreeView();
        }

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

            UpdateTreeView();
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {

            int x = WinAPI.GetCursorPosition().X, y = WinAPI.GetCursorPosition().Y;
            timeDown = (decimal)DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond - timeDown;

            switch (e.KeyData.ToString().ToUpper())
            {
                case "S":
                    //mr.AddCommandPress(x, y);
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
                case "O":

                    break;
                case "P":

                    break;
                case "Q":
                    MouseRobot.Instance.EmptyScript();
                    break;
            }

            keyDown = false;
            UpdateTreeView();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MouseRobot.Instance.StopScript();
        }

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

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MouseRobot.Instance.NewScript();
            UpdateTreeView();
        }

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtension.Script);
            openDialog.Title = "Select a script file to load.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                MouseRobot.Instance.OpenScript(openDialog.FileName);
            }
            UpdateTreeView();
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtension.Script);
            saveDialog.Title = "Select a script file to load.";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MouseRobot.Instance.SaveScript(saveDialog.FileName);
            }
            UpdateTreeView();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Dispose();
            GC.Collect();
            Application.Exit();
        }

        private void UpdateTreeView()
        {
            Console.WriteLine("Clear & Update tree view");
            treeView.Nodes.Clear();

            foreach (var script in ScriptManager.Instance)
            {
                var addDirtyToName = (script.IsDirty) ? "*" : "";
                TreeNode scriptNode = new TreeNode(script.Name + addDirtyToName);
                scriptNode.ImageIndex = 0;
                scriptNode.SelectedImageIndex = 0;
                treeView.Nodes.Add(scriptNode);
                foreach (var c in script)
                {
                    var commandNode = scriptNode.Nodes.Add(c.ToString());
                    commandNode.ImageIndex = 1;
                    commandNode.SelectedImageIndex = 1;
                }
            }

            treeView.ExpandAll();
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.All);
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (targetNode == null)
            {
                if (draggedNode.Level == 0)
                    InsertNodeAfter(draggedNode, treeView.Nodes[treeView.Nodes.Count - 1]);
                if (draggedNode.Level == 1)
                    InsertNodeAfter(draggedNode, draggedNode.Parent.Nodes[draggedNode.Parent.Nodes.Count - 1]);
            }
            else
            {
                InsertNodeAfter(draggedNode, targetNode);
            }
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            var canRelease = CanReleaseDragAndDrop(e);

            if (e.KeyState == 9 && canRelease) // 9 = CTRL is held down
                e.Effect = DragDropEffects.Copy;
            else if (canRelease)
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void InsertNodeAfter(TreeNode node, TreeNode nodeAfter)
        {
            var roots = treeView.Nodes.Count;

            if (node.Level == 0)
            {
                nodeAfter = (nodeAfter.Level == 0) ? nodeAfter : nodeAfter.Parent;
                ScriptManager.Instance.MoveScriptAfter(node.Index, nodeAfter.Index);

                node.Remove();
                treeView.Nodes.Insert(nodeAfter.Index + 1, node);
            }
            else if (node.Level == 1 && nodeAfter.Level == 0)
            {
                ScriptManager.Instance.MoveCommandAfter(node.Index, -1, node.Parent.Index, nodeAfter.Index);

                node.Remove();
                nodeAfter.Nodes.Insert(0, node);
            }
            else if (node.Level == 1 && nodeAfter.Level == 1 && node.Parent == nodeAfter.Parent)
            {
                ScriptManager.Instance.MoveCommandAfter(node.Index, nodeAfter.Index, nodeAfter.Parent.Index);

                node.Remove();
                nodeAfter.Parent.Nodes.Insert(nodeAfter.Index + 1, node);
            }
            else // nodes have different parents
            {
                ScriptManager.Instance.MoveCommandAfter(node.Index, nodeAfter.Index, node.Parent.Index, nodeAfter.Parent.Index);
                node.Remove();
                nodeAfter.Parent.Nodes.Insert(nodeAfter.Index + 1, node);
            }

#if ENABLE_UI_TESTING
            TestIfTreeIsTheSameAsInScriptManager();
#endif
        }


#if ENABLE_UI_TESTING
        void TestIfTreeIsTheSameAsInScriptManager()
        {
            for (int i = 0; i < treeView.Nodes.Count; i++)
            {
                Debug.Assert(treeView.Nodes[i].Text.Contains(ScriptManager.Instance.LoadedScripts[i].Name), 
                    string.Format("Hierarchy script missmatch: i:{0}", i));

                for (int j = 0; j < treeView.Nodes[i].Nodes.Count; j++)
                {
                    Debug.Assert(treeView.Nodes[i].Nodes[j].Text.Equals(ScriptManager.Instance.LoadedScripts[i].Commands[j].Text),
                        string.Format("Hierarchy missmatch: i:{0} j:{1}", i, j));
                }
            }
        }
#endif


        /// <summary>
        /// Can only release drag and drop if items to others, not self. They only change positions, not level. 
        /// Also, it is possible to drag to blank space (null), to move that object there
        /// </summary>
        private bool CanReleaseDragAndDrop(DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            
            return (targetNode == null && draggedNode.NextNode != null) || (targetNode != null && targetNode != draggedNode && targetNode.Parent != draggedNode);
        }
    }
}
