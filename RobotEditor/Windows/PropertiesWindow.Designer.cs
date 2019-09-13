namespace RobotEditor
{
    partial class PropertiesWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesWindow));
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.recordingSettingsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.imageDetectionSettingsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.compilerSettingsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid.Size = new System.Drawing.Size(184, 361);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.UseCompatibleTextRendering = true;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.recordingSettingsToolStripMenuItem,
            this.imageDetectionSettingsToolStripMenuItem,
            this.compilerSettingsToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(174, 70);
            // 
            // recordingSettingsToolStripMenuItem
            // 
            this.recordingSettingsToolStripMenuItem.Name = "recordingSettingsToolStripMenuItem";
            this.recordingSettingsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.recordingSettingsToolStripMenuItem.Text = "Recording Settings";
            this.recordingSettingsToolStripMenuItem.Click += new System.EventHandler(this.recordingSettingsToolStripMenuItem_Click);
            // 
            // imageDetectionSettingsToolStripMenuItem
            // 
            this.imageDetectionSettingsToolStripMenuItem.Name = "imageDetectionSettingsToolStripMenuItem";
            this.imageDetectionSettingsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.imageDetectionSettingsToolStripMenuItem.Text = "Detection Settings";
            this.imageDetectionSettingsToolStripMenuItem.Click += new System.EventHandler(this.imageDetectionSettingsToolStripMenuItem_Click);
            // 
            // compilerSettingsToolStripMenuItem
            // 
            this.compilerSettingsToolStripMenuItem.Name = "compilerSettingsToolStripMenuItem";
            this.compilerSettingsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.compilerSettingsToolStripMenuItem.Text = "Compiler Settings";
            this.compilerSettingsToolStripMenuItem.Click += new System.EventHandler(this.compilerSettingsToolStripMenuItem_Click);
            // 
            // PropertiesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 361);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.propertyGrid);
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PropertiesWindow";
            this.Text = "Properties";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem recordingSettingsToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem imageDetectionSettingsToolStripMenuItem;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem compilerSettingsToolStripMenuItem;
    }
}