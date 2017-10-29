#define ENABLE_UI_TESTING

using Robot;
using RobotEditor;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.Utils
{
    public class ScriptTreeViewUtils
    {
        public static void UpdateTreeView(TreeView treeView)
        {
            Console.WriteLine("Clear & Update tree view");
            treeView.Nodes.Clear();

            foreach (var script in ScriptManager.Instance)
                AddExistingScriptToTreeView(treeView, script);

            treeView.ExpandAll();
            ASSERT_TreeViewIsTheSameAsInScriptManager(treeView);
        }

        public static void UpdateTreeNodeFonts(TreeView treeView)
        {
            int i = -1;
            foreach (var script in ScriptManager.Instance)
            {
                i++;
                treeView.Nodes[i].Text = script.Name + ((script.IsDirty) ? "*" : "");
                treeView.Nodes[i].NodeFont = Fonts.Default;

                if (script == ScriptManager.Instance.ActiveScript && script.IsDirty)
                    treeView.Nodes[i].NodeFont = Fonts.ActiveAndDirtyScript;//.AddFont(Fonts.ActiveScript);
                else if (script == ScriptManager.Instance.ActiveScript)
                    treeView.Nodes[i].NodeFont = Fonts.ActiveScript;
                else if (script.IsDirty)
                    treeView.Nodes[i].NodeFont = Fonts.DirtyScript;//.AddFont(Fonts.DirtyScript);
                else
                    treeView.Nodes[i].NodeFont = Fonts.Default;   
            }
        }

        public static void AddExistingScriptToTreeView(TreeView treeView, Script script)
        {
            TreeNode scriptNode = new TreeNode(script.Name + ((script.IsDirty) ? "*" : ""));
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

        #region MENU ITEMS
        public static void SaveAllScripts(TreeView treeView, bool silent = false)
        {
            foreach (var script in ScriptManager.Instance)
            {
                if (!script.IsDirty)
                    continue;

                if (script.Path != "")
                    ScriptManager.Instance.SaveScript(script, script.Path);
                else
                    SaveScript(script, treeView, updateUI: false);
            }

            UpdateTreeNodeFonts(treeView);
        }

        public static void SaveScript(Script script, TreeView treeView, bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Script);
            saveDialog.Title = "Select a path for script to save.";
            saveDialog.FileName = script.Name + FileExtensions.ScriptD;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ScriptManager.Instance.SaveScript(ScriptManager.Instance.ActiveScript, saveDialog.FileName);
                if (updateUI)
                    UpdateTreeNodeFonts(treeView);
            }
            
        }

        public static void OpenScript(TreeView treeView)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Script);
            openDialog.Title = "Select a script file to load.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                var script = ScriptManager.Instance.LoadScript(openDialog.FileName);
                // OnScriptLoaded event will take care of updating UI
            }
        }

        public static void DeleteSelectedTreeViewItem(TreeView treeView)
        {
            if (treeView.SelectedNode == null)
                return;

            if (treeView.SelectedNode.Level == 0)
                ScriptManager.Instance.RemoveScript(treeView.SelectedNode.Index);
            else
                ScriptManager.Instance.LoadedScripts[treeView.SelectedNode.Parent.Index].RemoveCommand(treeView.SelectedNode.Index);

            treeView.SelectedNode.Remove();

            UpdateTreeNodeFonts(treeView);
            ASSERT_TreeViewIsTheSameAsInScriptManager(treeView);
        }

        public static void DuplicateSelectedTreeViewItem(TreeView treeView)
        {
            if (treeView.SelectedNode == null)
                return;

            TreeNode clone;
            if (treeView.SelectedNode.Level == 0)
            {
                var s = ScriptManager.Instance.NewScript(ScriptManager.Instance.LoadedScripts[treeView.SelectedNode.Index]);
                ScriptManager.Instance.MoveScriptAfter(ScriptManager.Instance.LoadedScripts.Count - 1, treeView.SelectedNode.Index);

                clone = (TreeNode)treeView.SelectedNode.Clone();
                clone.Text = s.Name;
                treeView.Nodes.Insert(treeView.SelectedNode.Index + 1, clone);
            }
            else
            {
                var c = (Command)ScriptManager.Instance.LoadedScripts[treeView.SelectedNode.Parent.Index].Commands[treeView.SelectedNode.Index].Clone();
                ScriptManager.Instance.LoadedScripts[treeView.SelectedNode.Parent.Index].InsertCommand(treeView.SelectedNode.Index + 1, c);

                clone = (TreeNode)treeView.SelectedNode.Clone();
                treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.Index + 1, clone);
            }

            treeView.SelectedNode = clone;
            treeView.Focus();

            UpdateTreeNodeFonts(treeView);
            ASSERT_TreeViewIsTheSameAsInScriptManager(treeView);
        }

        public static void ShowSelectedTreeViewItemInExplorer(TreeView treeView)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 0)
                return;

            var path = ScriptManager.Instance.LoadedScripts[treeView.SelectedNode.Index].Path;
            Process.Start("explorer.exe", "/select, " + path);
        }

        public static void SetSelectedScriptActive(TreeView treeView)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 0)
                return;

            ScriptManager.Instance.ActiveScript = ScriptManager.Instance.LoadedScripts[treeView.SelectedNode.Index];
            UpdateTreeNodeFonts(treeView);

            ASSERT_TreeViewIsTheSameAsInScriptManager(treeView);
        }

        public static void NewScript(TreeView treeView)
        {
            var script = ScriptManager.Instance.NewScript();
            AddExistingScriptToTreeView(treeView, script);
            UpdateTreeNodeFonts(treeView);

            ASSERT_TreeViewIsTheSameAsInScriptManager(treeView);
        }
        #endregion

        #region DRAG AND DROP
        public static void TreeView_DragDrop(TreeView treeView, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (targetNode == null)
            {
                if (draggedNode.Level == 0)
                    InsertNodeAfter(treeView, draggedNode, treeView.Nodes[treeView.Nodes.Count - 1]);
                if (draggedNode.Level == 1)
                    InsertNodeAfter(treeView, draggedNode, draggedNode.Parent.Nodes[draggedNode.Parent.Nodes.Count - 1]);
            }
            else
            {
                InsertNodeAfter(treeView, draggedNode, targetNode);
            }
        }

        public static void TreeView_DragOver(TreeView treeView, DragEventArgs e)
        {
            var canRelease = CanReleaseDragAndDrop(treeView, e);

            if (e.KeyState == 9 && canRelease) // 9 = CTRL is held down
                e.Effect = DragDropEffects.Copy;
            else if (canRelease)
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private static bool CanReleaseDragAndDrop(TreeView treeView, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            return (targetNode == null && draggedNode.NextNode != null) || (targetNode != null && targetNode != draggedNode && targetNode.Parent != draggedNode);
        }

        private static void InsertNodeAfter(TreeView treeView, TreeNode node, TreeNode nodeAfter)
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

            UpdateTreeNodeFonts(treeView);
            ASSERT_TreeViewIsTheSameAsInScriptManager(treeView);
        }
        #endregion


        private static void ASSERT_TreeViewIsTheSameAsInScriptManager(TreeView treeView)
        {
#if ENABLE_UI_TESTING
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
#endif
        }
    }
}
