﻿namespace RobotEditor
{
    partial class TestFixtureWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestFixtureWindow));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newRecordingToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.duplicateToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.createToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
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
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRecordingToolStripMenuItem1,
            this.toolStripSeparator1,
            this.duplicateToolStripMenuItem1,
            this.deleteToolStripMenuItem1,
            this.toolStripMenuItem2,
            this.createToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(167, 104);
            // 
            // newRecordingToolStripMenuItem1
            // 
            this.newRecordingToolStripMenuItem1.Name = "newRecordingToolStripMenuItem1";
            this.newRecordingToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newRecordingToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.newRecordingToolStripMenuItem1.Text = "New Test";
            this.newRecordingToolStripMenuItem1.Click += new System.EventHandler(this.newRecordingToolStripMenuItem1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(163, 6);
            // 
            // duplicateToolStripMenuItem1
            // 
            this.duplicateToolStripMenuItem1.Name = "duplicateToolStripMenuItem1";
            this.duplicateToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.duplicateToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.duplicateToolStripMenuItem1.Text = "Duplicate";
            this.duplicateToolStripMenuItem1.Click += new System.EventHandler(this.duplicateToolStripMenuItem1_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(163, 6);
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            this.createToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.createToolStripMenuItem.Text = "Create";
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "Recording_ICO_16.png");
            this.imageList.Images.SetKeyName(1, "gear_ICO_512.png");
            this.imageList.Images.SetKeyName(2, "Script_ICO_256.png");
            // 
            // treeListView
            // 
            this.treeListView.AllowDrop = true;
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.ContextMenuStrip = this.contextMenuStrip;
            this.treeListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.treeListView.HideSelection = false;
            this.treeListView.LargeImageList = this.imageList;
            this.treeListView.Location = new System.Drawing.Point(0, 25);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.ShowImagesOnSubItems = true;
            this.treeListView.Size = new System.Drawing.Size(184, 336);
            this.treeListView.SmallImageList = this.imageList;
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
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
            this.toolStrip.TabIndex = 2;
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
            // 
            // TestFixtureWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 361);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.treeListView);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestFixtureWindow";
            this.Text = "TestFixture";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestFixtureWindow_FormClosing);
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem newRecordingToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem duplicateToolStripMenuItem1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.ImageList imageList;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        public BrightIdeasSoftware.TreeListView treeListView;
        private RobotEditor.CustomControls.TrackedToolStripButton ToolstripExpandAll;
        private RobotEditor.CustomControls.TrackedToolStripButton ToolstripExpandOne;
        private RobotEditor.CustomControls.TrackedToolStripButton ToolstripCollapseAll;
        internal System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem createToolStripMenuItem;
    }
}