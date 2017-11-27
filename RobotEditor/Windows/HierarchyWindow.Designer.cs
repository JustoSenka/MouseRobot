namespace RobotEditor
{
    partial class HierarchyWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HierarchyWindow));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setActiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newScriptToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.duplicateToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.treeListView = new BrightIdeasSoftware.TreeListView();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setActiveToolStripMenuItem,
            this.newScriptToolStripMenuItem1,
            this.toolStripSeparator1,
            this.showInExplorerToolStripMenuItem,
            this.toolStripMenuItem1,
            this.duplicateToolStripMenuItem1,
            this.deleteToolStripMenuItem1});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(175, 126);
            // 
            // setActiveToolStripMenuItem
            // 
            this.setActiveToolStripMenuItem.Name = "setActiveToolStripMenuItem";
            this.setActiveToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.setActiveToolStripMenuItem.Text = "Set Active";
            this.setActiveToolStripMenuItem.Click += new System.EventHandler(this.setActiveToolStripMenuItem_Click);
            // 
            // newScriptToolStripMenuItem1
            // 
            this.newScriptToolStripMenuItem1.Name = "newScriptToolStripMenuItem1";
            this.newScriptToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newScriptToolStripMenuItem1.Size = new System.Drawing.Size(174, 22);
            this.newScriptToolStripMenuItem1.Text = "New Script";
            this.newScriptToolStripMenuItem1.Click += new System.EventHandler(this.newScriptToolStripMenuItem1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(171, 6);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.showInExplorerToolStripMenuItem.Text = "Show in explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(171, 6);
            // 
            // duplicateToolStripMenuItem1
            // 
            this.duplicateToolStripMenuItem1.Name = "duplicateToolStripMenuItem1";
            this.duplicateToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.duplicateToolStripMenuItem1.Size = new System.Drawing.Size(174, 22);
            this.duplicateToolStripMenuItem1.Text = "Duplicate";
            this.duplicateToolStripMenuItem1.Click += new System.EventHandler(this.duplicateToolStripMenuItem1_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(174, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "script_ICO_256.png");
            this.imageList.Images.SetKeyName(1, "gear_ICO_512.png");
            // 
            // treeListView
            // 
            this.treeListView.AllowDrop = true;
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.ContextMenuStrip = this.contextMenuStrip;
            this.treeListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.treeListView.LargeImageList = this.imageList;
            this.treeListView.Location = new System.Drawing.Point(0, 0);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.ShowImagesOnSubItems = true;
            this.treeListView.Size = new System.Drawing.Size(469, 432);
            this.treeListView.SmallImageList = this.imageList;
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            this.treeListView.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.treeListView_ModelCanDrop);
            this.treeListView.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.treeListView_ModelDropped);
            this.treeListView.SelectionChanged += new System.EventHandler(this.treeListView_SelectionChanged);
            // 
            // HierarchyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 432);
            this.Controls.Add(this.treeListView);
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HierarchyWindow";
            this.Text = "Hierarchy";
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem setActiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newScriptToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.ImageList imageList;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        public BrightIdeasSoftware.TreeListView treeListView;
    }
}