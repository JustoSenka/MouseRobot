using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseRobot;

namespace MouseRobotUI
{
    public partial class TreeViewForm : Form
    {
        Lazy<IMouseRobot> lazyMR = DependencyInjector.GetLazyMouseRobot();

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
                    lazyMR.Value.AddCommandPress(x, y);
                    break;
                case "D":
                    lazyMR.Value.AddCommandPress(x, y);
                    lazyMR.Value.AddCommandSleep((int)timeDown);
                    Console.WriteLine("Sleep for..." + timeDown);
                    break;
                case "F":
                    lazyMR.Value.AddCommandSleep((int)timeDown);
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
                    lazyMR.Value.AddCommandSleep(sleepTime);
                    break;
                case "H":
                    lazyMR.Value.AddCommandDown(x, y);
                    break;
                case "J":
                    lazyMR.Value.AddCommandMove(x, y);
                    break;
                case "K":
                    lazyMR.Value.AddCommandRelease();
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
                    lazyMR.Value.EmptyScript();
                    break;
            }

            keyDown = false;
            UpdateTreeView();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lazyMR.Value.StopScript();
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
                lazyMR.Value.StartScript(repeatTimes);
            }
            catch (EmptyScriptException ese)
            {
                Console.WriteLine(ese.Message);
            }
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lazyMR.Value.NewScript();
            UpdateTreeView();
        }

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
            openDialog.Title = "Select a script file to load.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                lazyMR.Value.OpenScript(openDialog.FileName);
            }
            UpdateTreeView();
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
            saveDialog.Title = "Select a script file to load.";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                lazyMR.Value.SaveScript(saveDialog.FileName);
            }
            UpdateTreeView();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Dispose();
            lazyMR = null;
            GC.Collect();
            Application.Exit();
        }

        private void UpdateTreeView()
        {
            Console.WriteLine("Clear & Update tree view");
            treeView.Nodes.Clear();

            var tree = lazyMR.Value.GetScriptTreeStructure();
            foreach (var script in tree)
            {
                TreeNode scriptNode = new TreeNode(script.value);
                scriptNode.ImageIndex = 0;
                scriptNode.SelectedImageIndex = 0;
                treeView.Nodes.Add(scriptNode);
                foreach (var c in script)
                {
                    var commandNode = scriptNode.Nodes.Add(c.value);
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
                node.Remove();
                treeView.Nodes.Insert(nodeAfter.Index + 1, node);
            }
            else if (node.Level == 1 && nodeAfter.Level == 0)
            {
                node.Remove();
                nodeAfter.Nodes.Insert(0, node);
            }
            else if (node.Level == 1 && nodeAfter.Level == 1 && node.Parent == nodeAfter.Parent)
            {
                node.Remove();
                nodeAfter.Parent.Nodes.Insert(nodeAfter.Index + 1, node);
            }
            else // nodes have different parents
            {
                //var parent = nodeAfter.Parent;
                node.Remove();
                nodeAfter.Parent.Nodes.Insert(nodeAfter.Index + 1, node);
            }
        }

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
