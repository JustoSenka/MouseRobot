﻿using System;
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
            this.reloadRecordingToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.reloadFixtureToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.deleteToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.newFolderToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.addScriptToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.recompileRecordingsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.regenerateSolutionToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.showInExplorerToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.treeListView = new BrightIdeasSoftware.TreeListView();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.ToolstripExpandAll = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.ToolstripExpandOne = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.ToolstripCollapseAll = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reloadRecordingToolStripMenuItem,
            this.reloadFixtureToolStripMenuItem,
            this.toolStripMenuItem2,
            this.refreshToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.newFolderToolStripMenuItem,
            this.toolStripMenuItem1,
            this.addScriptToolStripMenuItem,
            this.recompileRecordingsToolStripMenuItem,
            this.regenerateSolutionToolStripMenuItem,
            this.toolStripMenuItem3,
            this.showInExplorerToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(221, 220);
            // 
            // reloadRecordingToolStripMenuItem
            // 
            this.reloadRecordingToolStripMenuItem.Name = "reloadRecordingToolStripMenuItem";
            this.reloadRecordingToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.reloadRecordingToolStripMenuItem.Text = "Reload Recording";
            this.reloadRecordingToolStripMenuItem.Click += new System.EventHandler(this.reloadRecordingToolStripMenuItem_Click);
            // 
            // reloadFixtureToolStripMenuItem
            // 
            this.reloadFixtureToolStripMenuItem.Name = "reloadFixtureToolStripMenuItem";
            this.reloadFixtureToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.reloadFixtureToolStripMenuItem.Text = "Reload Fixture";
            this.reloadFixtureToolStripMenuItem.Click += new System.EventHandler(this.reloadFixtureToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(217, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // newFolderToolStripMenuItem
            // 
            this.newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            this.newFolderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newFolderToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.newFolderToolStripMenuItem.Text = "New Folder";
            this.newFolderToolStripMenuItem.Click += new System.EventHandler(this.newFolderToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(217, 6);
            // 
            // addScriptToolStripMenuItem
            // 
            this.addScriptToolStripMenuItem.Name = "addScriptToolStripMenuItem";
            this.addScriptToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.addScriptToolStripMenuItem.Text = "Add Script";
            // 
            // recompileRecordingsToolStripMenuItem
            // 
            this.recompileRecordingsToolStripMenuItem.Name = "recompileRecordingsToolStripMenuItem";
            this.recompileRecordingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
            this.recompileRecordingsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.recompileRecordingsToolStripMenuItem.Text = "Recompile Scripts";
            this.recompileRecordingsToolStripMenuItem.Click += new System.EventHandler(this.recompileRecordingsToolStripMenuItem_Click);
            // 
            // regenerateSolutionToolStripMenuItem
            // 
            this.regenerateSolutionToolStripMenuItem.Name = "regenerateSolutionToolStripMenuItem";
            this.regenerateSolutionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.regenerateSolutionToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.regenerateSolutionToolStripMenuItem.Text = "Regenerate Solution";
            this.regenerateSolutionToolStripMenuItem.Click += new System.EventHandler(this.regenerateSolutionToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(217, 6);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.showInExplorerToolStripMenuItem.Text = "Show In Explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FailedImport_ICO_16.png");
            this.imageList1.Images.SetKeyName(1, "Folder_ICO_128.png");
            this.imageList1.Images.SetKeyName(2, "Recording_ICO_16.png");
            this.imageList1.Images.SetKeyName(3, "Image_ICO_128.png");
            this.imageList1.Images.SetKeyName(4, "CSharpScript_ICO_16.png");
            this.imageList1.Images.SetKeyName(5, "Test_ICO_256.png");
            this.imageList1.Images.SetKeyName(6, "Plugn_ICO_16.png");
            // 
            // treeListView
            // 
            this.treeListView.AllowDrop = true;
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.ContextMenuStrip = this.contextMenuStrip;
            this.treeListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.treeListView.HideSelection = false;
            this.treeListView.LargeImageList = this.imageList1;
            this.treeListView.Location = new System.Drawing.Point(0, 25);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.ShowImagesOnSubItems = true;
            this.treeListView.Size = new System.Drawing.Size(184, 336);
            this.treeListView.SmallImageList = this.imageList1;
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            this.treeListView.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.treeListView_ModelCanDrop);
            this.treeListView.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.treeListView_ModelDropped);
            this.treeListView.SelectionChanged += new System.EventHandler(this.treeListView_SelectionChanged);
            this.treeListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeListView_MouseDoubleClick);
            this.treeListView.Resize += new System.EventHandler(this.treeListView_Resize);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolstripExpandAll,
            this.ToolstripExpandOne,
            this.ToolstripCollapseAll});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.toolStrip.Size = new System.Drawing.Size(184, 25);
            this.toolStrip.TabIndex = 3;
            this.toolStrip.Text = "toolStrip";
            // 
            // ToolstripExpandAll
            // 
            this.ToolstripExpandAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolstripExpandAll.Image = global::RobotEditor.Properties.Resources.ExpandAll_16;
            this.ToolstripExpandAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolstripExpandAll.Name = "ToolstripExpandAll";
            this.ToolstripExpandAll.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.ToolstripExpandAll.Size = new System.Drawing.Size(40, 22);
            this.ToolstripExpandAll.Text = "toolStripButton1";
            this.ToolstripExpandAll.ToolTipText = "Expand All Items";
            this.ToolstripExpandAll.Click += new System.EventHandler(this.ToolstripExpandAll_Click);
            // 
            // ToolstripExpandOne
            // 
            this.ToolstripExpandOne.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolstripExpandOne.Image = global::RobotEditor.Properties.Resources.ExpandOne_16;
            this.ToolstripExpandOne.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolstripExpandOne.Name = "ToolstripExpandOne";
            this.ToolstripExpandOne.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.ToolstripExpandOne.Size = new System.Drawing.Size(40, 22);
            this.ToolstripExpandOne.Text = "toolStripButton2";
            this.ToolstripExpandOne.ToolTipText = "Expand First Level";
            this.ToolstripExpandOne.Click += new System.EventHandler(this.ToolstripExpandOne_Click);
            // 
            // ToolstripCollapseAll
            // 
            this.ToolstripCollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolstripCollapseAll.Image = global::RobotEditor.Properties.Resources.CollapseAll_16;
            this.ToolstripCollapseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolstripCollapseAll.Name = "ToolstripCollapseAll";
            this.ToolstripCollapseAll.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.ToolstripCollapseAll.Size = new System.Drawing.Size(40, 22);
            this.ToolstripCollapseAll.Text = "toolStripButton3";
            this.ToolstripCollapseAll.ToolTipText = "Collapse All Items";
            this.ToolstripCollapseAll.Click += new System.EventHandler(this.ToolstripCollapseAll_Click);
            // 
            // AssetsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 361);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.treeListView);
            this.Controls.Add(this.toolStrip);
            this.HideOnClose = true;
            this.Name = "AssetsWindow";
            this.Text = "Assets";
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private RobotEditor.CustomControls.TrackedToolStripMenuItem refreshToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem showInExplorerToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem reloadRecordingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem recompileRecordingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem regenerateSolutionToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem addScriptToolStripMenuItem;
        private BrightIdeasSoftware.TreeListView treeListView;
        internal System.Windows.Forms.ToolStrip toolStrip;
        private RobotEditor.CustomControls.TrackedToolStripButton ToolstripExpandAll;
        private RobotEditor.CustomControls.TrackedToolStripButton ToolstripExpandOne;
        private RobotEditor.CustomControls.TrackedToolStripButton ToolstripCollapseAll;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem newFolderToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem reloadFixtureToolStripMenuItem;
    }
}