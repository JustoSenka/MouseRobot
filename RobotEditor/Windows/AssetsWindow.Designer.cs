using System;
using BrightIdeasSoftware;

namespace RobotEditor
{
    partial class AssetsWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetsWindow));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.reloadRecordingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.addScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recompileRecordingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regenerateSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.showInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.treeListView = new BrightIdeasSoftware.TreeListView();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reloadRecordingToolStripMenuItem,
            this.toolStripMenuItem2,
            this.refreshToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.addScriptToolStripMenuItem,
            this.recompileRecordingsToolStripMenuItem,
            this.regenerateSolutionToolStripMenuItem,
            this.toolStripMenuItem3,
            this.showInExplorerToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(234, 176);
            // 
            // reloadRecordingToolStripMenuItem
            // 
            this.reloadRecordingToolStripMenuItem.Name = "reloadRecordingToolStripMenuItem";
            this.reloadRecordingToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.reloadRecordingToolStripMenuItem.Text = "Reload Recording";
            this.reloadRecordingToolStripMenuItem.Click += new System.EventHandler(this.reloadRecordingToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(230, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(230, 6);
            // 
            // addScriptToolStripMenuItem
            // 
            this.addScriptToolStripMenuItem.Name = "addScriptToolStripMenuItem";
            this.addScriptToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.addScriptToolStripMenuItem.Text = "Add Script";
            // 
            // recompileRecordingsToolStripMenuItem
            // 
            this.recompileRecordingsToolStripMenuItem.Name = "recompileRecordingsToolStripMenuItem";
            this.recompileRecordingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
            this.recompileRecordingsToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.recompileRecordingsToolStripMenuItem.Text = "Recompile Recordings";
            this.recompileRecordingsToolStripMenuItem.Click += new System.EventHandler(this.recompileRecordingsToolStripMenuItem_Click);
            // 
            // regenerateSolutionToolStripMenuItem
            // 
            this.regenerateSolutionToolStripMenuItem.Name = "regenerateSolutionToolStripMenuItem";
            this.regenerateSolutionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.regenerateSolutionToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.regenerateSolutionToolStripMenuItem.Text = "Regenerate Solution";
            this.regenerateSolutionToolStripMenuItem.Click += new System.EventHandler(this.regenerateSolutionToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(230, 6);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showInExplorerToolStripMenuItem.Text = "Show In Explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Folder_ICO_128.png");
            this.imageList1.Images.SetKeyName(1, "Recording_ICO_16.png");
            this.imageList1.Images.SetKeyName(2, "Image_ICO_128.png");
            this.imageList1.Images.SetKeyName(3, "Script_ICO_256.png");
            // 
            // treeListView
            // 
            this.treeListView.AllowDrop = true;
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.ContextMenuStrip = this.contextMenuStrip;
            this.treeListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.treeListView.LargeImageList = this.imageList1;
            this.treeListView.Location = new System.Drawing.Point(0, 0);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.ShowImagesOnSubItems = true;
            this.treeListView.Size = new System.Drawing.Size(402, 372);
            this.treeListView.SmallImageList = this.imageList1;
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            this.treeListView.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.treeListView_ModelCanDrop);
            this.treeListView.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.treeListView_ModelDropped);
            this.treeListView.SelectionChanged += new System.EventHandler(this.treeListView_SelectionChanged);
            this.treeListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeListView_MouseDoubleClick);
            // 
            // AssetsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 372);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.treeListView);
            this.HideOnClose = true;
            this.Name = "AssetsWindow";
            this.Text = "Assets";
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem reloadRecordingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem recompileRecordingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem regenerateSolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addScriptToolStripMenuItem;
        private BrightIdeasSoftware.TreeListView treeListView;
    }
}