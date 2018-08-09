namespace RobotEditor
{
    partial class TestRunnerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestRunnerWindow));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.runSelectedTestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.showInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.treeListView = new BrightIdeasSoftware.TreeListView();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.RunAllButton = new System.Windows.Forms.ToolStripButton();
            this.RunDropdownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.runSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runFailedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runNotRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopRunButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolstripExpandAll = new System.Windows.Forms.ToolStripButton();
            this.ToolstripExpandOne = new System.Windows.Forms.ToolStripButton();
            this.ToolstripCollapseAll = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runSelectedTestsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.showInExplorerToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(181, 76);
            // 
            // runSelectedTestsToolStripMenuItem
            // 
            this.runSelectedTestsToolStripMenuItem.Name = "runSelectedTestsToolStripMenuItem";
            this.runSelectedTestsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.runSelectedTestsToolStripMenuItem.Text = "Run selected tests";
            this.runSelectedTestsToolStripMenuItem.Click += new System.EventHandler(this.runSelectedToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showInExplorerToolStripMenuItem.Text = "Show in explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "TestStatus_info_32.png");
            this.imageList.Images.SetKeyName(1, "TestStatus_Pass_32.png");
            this.imageList.Images.SetKeyName(2, "TestStatus_Fail_32.png");
            // 
            // treeListView
            // 
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.ContextMenuStrip = this.contextMenuStrip;
            this.treeListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.treeListView.LargeImageList = this.imageList;
            this.treeListView.Location = new System.Drawing.Point(0, 25);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.ShowImagesOnSubItems = true;
            this.treeListView.Size = new System.Drawing.Size(469, 407);
            this.treeListView.SmallImageList = this.imageList;
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            this.treeListView.DoubleClick += new System.EventHandler(this.treeListView_DoubleClick);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RunAllButton,
            this.RunDropdownButton,
            this.StopRunButton,
            this.toolStripSeparator1,
            this.ToolstripExpandAll,
            this.ToolstripExpandOne,
            this.ToolstripCollapseAll});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.toolStrip.Size = new System.Drawing.Size(469, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip";
            // 
            // RunAllButton
            // 
            this.RunAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.RunAllButton.Image = ((System.Drawing.Image)(resources.GetObject("RunAllButton.Image")));
            this.RunAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RunAllButton.Name = "RunAllButton";
            this.RunAllButton.Size = new System.Drawing.Size(49, 22);
            this.RunAllButton.Text = "Run All";
            this.RunAllButton.Click += new System.EventHandler(this.RunAllButton_Click);
            // 
            // RunDropdownButton
            // 
            this.RunDropdownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.RunDropdownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runSelectedToolStripMenuItem,
            this.runFailedToolStripMenuItem,
            this.runNotRunToolStripMenuItem});
            this.RunDropdownButton.Image = ((System.Drawing.Image)(resources.GetObject("RunDropdownButton.Image")));
            this.RunDropdownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RunDropdownButton.Name = "RunDropdownButton";
            this.RunDropdownButton.Size = new System.Drawing.Size(50, 22);
            this.RunDropdownButton.Text = "Run...";
            // 
            // runSelectedToolStripMenuItem
            // 
            this.runSelectedToolStripMenuItem.Name = "runSelectedToolStripMenuItem";
            this.runSelectedToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.runSelectedToolStripMenuItem.Text = "Run Selected Tests";
            this.runSelectedToolStripMenuItem.Click += new System.EventHandler(this.runSelectedToolStripMenuItem_Click);
            // 
            // runFailedToolStripMenuItem
            // 
            this.runFailedToolStripMenuItem.Name = "runFailedToolStripMenuItem";
            this.runFailedToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.runFailedToolStripMenuItem.Text = "Run Failed Tests";
            this.runFailedToolStripMenuItem.Click += new System.EventHandler(this.runFailedToolStripMenuItem_Click);
            // 
            // runNotRunToolStripMenuItem
            // 
            this.runNotRunToolStripMenuItem.Name = "runNotRunToolStripMenuItem";
            this.runNotRunToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.runNotRunToolStripMenuItem.Text = "Run Not Run tests";
            this.runNotRunToolStripMenuItem.Click += new System.EventHandler(this.runNotRunToolStripMenuItem_Click);
            // 
            // StopRunButton
            // 
            this.StopRunButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StopRunButton.Enabled = false;
            this.StopRunButton.Image = ((System.Drawing.Image)(resources.GetObject("StopRunButton.Image")));
            this.StopRunButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StopRunButton.Name = "StopRunButton";
            this.StopRunButton.Size = new System.Drawing.Size(35, 22);
            this.StopRunButton.Text = "Stop";
            this.StopRunButton.Click += new System.EventHandler(this.StopRunButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
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
            // TestRunnerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 432);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.treeListView);
            this.Controls.Add(this.toolStrip);
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestRunnerWindow";
            this.Text = "Test Runner";
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem showInExplorerToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        public BrightIdeasSoftware.TreeListView treeListView;
        private System.Windows.Forms.ToolStripButton ToolstripExpandAll;
        private System.Windows.Forms.ToolStripButton ToolstripExpandOne;
        private System.Windows.Forms.ToolStripButton ToolstripCollapseAll;
        internal System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripMenuItem runSelectedTestsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton StopRunButton;
        private System.Windows.Forms.ToolStripDropDownButton RunDropdownButton;
        private System.Windows.Forms.ToolStripMenuItem runSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runFailedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runNotRunToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton RunAllButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}