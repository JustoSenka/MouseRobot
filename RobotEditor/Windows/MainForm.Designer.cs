﻿namespace RobotEditor
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.m_DockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.visualStudioToolStripExtender = new WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(this.components);
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.leftLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.rightLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.vS2015DarkTheme1 = new WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme();
            this.vS2015BlueTheme1 = new WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme();
            this.vS2015LightTheme1 = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.playButton = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.recordButton = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.actionOnPlay = new RobotEditor.CustomControls.TrackedToolStripComboBox();
            this.actionOnRec = new RobotEditor.CustomControls.TrackedToolStripComboBox();
            this.visualizationButton = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.textDetectionButton = new RobotEditor.CustomControls.TrackedToolStripButton();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.newToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.newProjectToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.newTestFixtureToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.newRecordingToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.openProjectToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAllRecordingsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.saveRecordingToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.addScriptToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.openRecordingToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem1 = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.editToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.duplicateToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.deleteToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.optionsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.darkThemeToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.blueThemeToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.whiteThemeToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.windowToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.commandsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.recordingToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.imageDetectionToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.compilerToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.testRunnerToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.hierarchyToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.imagePreviewToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.assetsToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.profilerToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.inspectorToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.consoleToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.helpToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.aboutToolStripMenuItem = new RobotEditor.CustomControls.TrackedToolStripMenuItem();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.resetDefaultLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // m_DockPanel
            // 
            this.m_DockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_DockPanel.DockBackColor = System.Drawing.SystemColors.AppWorkspace;
            this.m_DockPanel.DockBottomPortion = 150D;
            this.m_DockPanel.DockLeftPortion = 200D;
            this.m_DockPanel.DockRightPortion = 200D;
            this.m_DockPanel.DockTopPortion = 150D;
            this.m_DockPanel.Location = new System.Drawing.Point(0, 49);
            this.m_DockPanel.Name = "m_DockPanel";
            this.m_DockPanel.Size = new System.Drawing.Size(763, 423);
            this.m_DockPanel.TabIndex = 0;
            // 
            // visualStudioToolStripExtender
            // 
            this.visualStudioToolStripExtender.DefaultRenderer = null;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.leftLabel,
            this.rightLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 472);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(763, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip1";
            // 
            // leftLabel
            // 
            this.leftLabel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.leftLabel.Name = "leftLabel";
            this.leftLabel.Size = new System.Drawing.Size(374, 17);
            this.leftLabel.Spring = true;
            this.leftLabel.Text = "test";
            this.leftLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rightLabel
            // 
            this.rightLabel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.rightLabel.Name = "rightLabel";
            this.rightLabel.Size = new System.Drawing.Size(374, 17);
            this.rightLabel.Spring = true;
            this.rightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playButton,
            this.recordButton,
            this.actionOnPlay,
            this.actionOnRec,
            this.visualizationButton,
            this.textDetectionButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(763, 25);
            this.toolStrip.TabIndex = 7;
            this.toolStrip.Text = "Tool Strip";
            // 
            // playButton
            // 
            this.playButton.AnalyticsEnabled = true;
            this.playButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playButton.Image = global::RobotEditor.Properties.Resources.ToolButton_Play_32;
            this.playButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(23, 22);
            this.playButton.Text = "Play (F1)";
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // recordButton
            // 
            this.recordButton.AnalyticsEnabled = true;
            this.recordButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.recordButton.Image = global::RobotEditor.Properties.Resources.ToolButton_Record_32;
            this.recordButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(23, 22);
            this.recordButton.Text = "Start Recording (F2)";
            this.recordButton.Click += new System.EventHandler(this.recordButton_Click);
            // 
            // actionOnPlay
            // 
            this.actionOnPlay.AnalyticsEnabled = true;
            this.actionOnPlay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionOnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.actionOnPlay.Items.AddRange(new object[] {
            "Do Nothing On Play",
            "Windowed On Play",
            "Minimize On Play"});
            this.actionOnPlay.Name = "actionOnPlay";
            this.actionOnPlay.Size = new System.Drawing.Size(140, 25);
            this.actionOnPlay.SelectedIndexChanged += new System.EventHandler(this.actionOnPlay_SelectedIndexChanged);
            // 
            // actionOnRec
            // 
            this.actionOnRec.AnalyticsEnabled = true;
            this.actionOnRec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionOnRec.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.actionOnRec.Items.AddRange(new object[] {
            "Do Nothing On Rec",
            "Windowed On Rec",
            "Minimize On Rec"});
            this.actionOnRec.Name = "actionOnRec";
            this.actionOnRec.Size = new System.Drawing.Size(140, 25);
            this.actionOnRec.SelectedIndexChanged += new System.EventHandler(this.actionOnRec_SelectedIndexChanged);
            // 
            // visualizationButton
            // 
            this.visualizationButton.AnalyticsEnabled = true;
            this.visualizationButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.visualizationButton.Image = global::RobotEditor.Properties.Resources.Eye_d_ICO_256;
            this.visualizationButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.visualizationButton.Name = "visualizationButton";
            this.visualizationButton.Size = new System.Drawing.Size(23, 22);
            this.visualizationButton.Text = "Enable realtime Image Detection visualization";
            this.visualizationButton.ToolTipText = "Enable Realtime Image Detection Visualization";
            this.visualizationButton.Click += new System.EventHandler(this.enableVizualization_Click);
            // 
            // textDetectionButton
            // 
            this.textDetectionButton.AnalyticsEnabled = true;
            this.textDetectionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.textDetectionButton.Image = global::RobotEditor.Properties.Resources.Text_d_ICO_32;
            this.textDetectionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.textDetectionButton.Name = "textDetectionButton";
            this.textDetectionButton.Size = new System.Drawing.Size(23, 22);
            this.textDetectionButton.Text = "Enable Realtime Text Detection visualization";
            this.textDetectionButton.ToolTipText = "Enable Realtime Text Detection Visualization";
            this.textDetectionButton.Click += new System.EventHandler(this.textDetectionButton_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(763, 24);
            this.menuStrip.TabIndex = 17;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.AnalyticsEnabled = true;
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openProjectToolStripMenuItem,
            this.toolStripMenuItem2,
            this.saveAllRecordingsToolStripMenuItem,
            this.saveRecordingToolStripMenuItem,
            this.toolStripMenuItem1,
            this.addScriptToolStripMenuItem,
            this.openRecordingToolStripMenuItem,
            this.exitToolStripMenuItem,
            this.exitToolStripMenuItem1});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.AnalyticsEnabled = true;
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newProjectToolStripMenuItem1,
            this.newTestFixtureToolStripMenuItem1,
            this.newRecordingToolStripMenuItem1});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // newProjectToolStripMenuItem1
            // 
            this.newProjectToolStripMenuItem1.AnalyticsEnabled = true;
            this.newProjectToolStripMenuItem1.Name = "newProjectToolStripMenuItem1";
            this.newProjectToolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
            this.newProjectToolStripMenuItem1.Text = "Project";
            this.newProjectToolStripMenuItem1.Click += new System.EventHandler(this.newProjectToolStripMenuItem_Click);
            // 
            // newTestFixtureToolStripMenuItem1
            // 
            this.newTestFixtureToolStripMenuItem1.AnalyticsEnabled = true;
            this.newTestFixtureToolStripMenuItem1.Name = "newTestFixtureToolStripMenuItem1";
            this.newTestFixtureToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.newTestFixtureToolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
            this.newTestFixtureToolStripMenuItem1.Text = "Test Fixture";
            this.newTestFixtureToolStripMenuItem1.Click += new System.EventHandler(this.newTestFixtureToolStripMenuItem_Click);
            // 
            // newRecordingToolStripMenuItem1
            // 
            this.newRecordingToolStripMenuItem1.AnalyticsEnabled = true;
            this.newRecordingToolStripMenuItem1.Name = "newRecordingToolStripMenuItem1";
            this.newRecordingToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newRecordingToolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
            this.newRecordingToolStripMenuItem1.Text = "Recording";
            this.newRecordingToolStripMenuItem1.Click += new System.EventHandler(this.newRecordingToolStripMenuItem_Click);
            // 
            // openProjectToolStripMenuItem
            // 
            this.openProjectToolStripMenuItem.AnalyticsEnabled = true;
            this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.openProjectToolStripMenuItem.Text = "Open Project";
            this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openProjectToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(186, 6);
            // 
            // saveAllRecordingsToolStripMenuItem
            // 
            this.saveAllRecordingsToolStripMenuItem.AnalyticsEnabled = true;
            this.saveAllRecordingsToolStripMenuItem.Name = "saveAllRecordingsToolStripMenuItem";
            this.saveAllRecordingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAllRecordingsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveAllRecordingsToolStripMenuItem.Text = "Save";
            this.saveAllRecordingsToolStripMenuItem.Click += new System.EventHandler(this.saveAllRecordingsToolStripMenuItem_Click);
            // 
            // saveRecordingToolStripMenuItem
            // 
            this.saveRecordingToolStripMenuItem.AnalyticsEnabled = true;
            this.saveRecordingToolStripMenuItem.Name = "saveRecordingToolStripMenuItem";
            this.saveRecordingToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveRecordingToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveRecordingToolStripMenuItem.Text = "Save As";
            this.saveRecordingToolStripMenuItem.Click += new System.EventHandler(this.saveRecordingToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(186, 6);
            // 
            // addScriptToolStripMenuItem
            // 
            this.addScriptToolStripMenuItem.AnalyticsEnabled = true;
            this.addScriptToolStripMenuItem.Name = "addScriptToolStripMenuItem";
            this.addScriptToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.addScriptToolStripMenuItem.Text = "Add Script";
            // 
            // openRecordingToolStripMenuItem
            // 
            this.openRecordingToolStripMenuItem.AnalyticsEnabled = true;
            this.openRecordingToolStripMenuItem.Name = "openRecordingToolStripMenuItem";
            this.openRecordingToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openRecordingToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.openRecordingToolStripMenuItem.Text = "Import Assets";
            this.openRecordingToolStripMenuItem.Click += new System.EventHandler(this.importAssetsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 6);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.AnalyticsEnabled = true;
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(189, 22);
            this.exitToolStripMenuItem1.Text = "Exit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.AnalyticsEnabled = true;
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.duplicateToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // duplicateToolStripMenuItem
            // 
            this.duplicateToolStripMenuItem.AnalyticsEnabled = true;
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            this.duplicateToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.duplicateToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.duplicateToolStripMenuItem.Text = "Duplicate";
            this.duplicateToolStripMenuItem.Click += new System.EventHandler(this.duplicateToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.AnalyticsEnabled = true;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.AnalyticsEnabled = true;
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.darkThemeToolStripMenuItem,
            this.blueThemeToolStripMenuItem,
            this.whiteThemeToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // darkThemeToolStripMenuItem
            // 
            this.darkThemeToolStripMenuItem.AnalyticsEnabled = true;
            this.darkThemeToolStripMenuItem.Name = "darkThemeToolStripMenuItem";
            this.darkThemeToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.darkThemeToolStripMenuItem.Text = "Dark Theme";
            this.darkThemeToolStripMenuItem.Click += new System.EventHandler(this.darkThemeToolStripMenuItem_Click);
            // 
            // blueThemeToolStripMenuItem
            // 
            this.blueThemeToolStripMenuItem.AnalyticsEnabled = true;
            this.blueThemeToolStripMenuItem.Name = "blueThemeToolStripMenuItem";
            this.blueThemeToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.blueThemeToolStripMenuItem.Text = "Blue Theme";
            this.blueThemeToolStripMenuItem.Click += new System.EventHandler(this.blueThemeToolStripMenuItem_Click);
            // 
            // whiteThemeToolStripMenuItem
            // 
            this.whiteThemeToolStripMenuItem.AnalyticsEnabled = true;
            this.whiteThemeToolStripMenuItem.Name = "whiteThemeToolStripMenuItem";
            this.whiteThemeToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.whiteThemeToolStripMenuItem.Text = "Light Theme";
            this.whiteThemeToolStripMenuItem.Click += new System.EventHandler(this.lightThemeToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.AnalyticsEnabled = true;
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commandsToolStripMenuItem,
            this.testRunnerToolStripMenuItem,
            this.hierarchyToolStripMenuItem,
            this.imagePreviewToolStripMenuItem,
            this.assetsToolStripMenuItem,
            this.profilerToolStripMenuItem,
            this.inspectorToolStripMenuItem,
            this.consoleToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.AnalyticsEnabled = true;
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.recordingToolStripMenuItem,
            this.imageDetectionToolStripMenuItem,
            this.compilerToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.commandsToolStripMenuItem.Text = "Settings";
            // 
            // recordingToolStripMenuItem
            // 
            this.recordingToolStripMenuItem.AnalyticsEnabled = true;
            this.recordingToolStripMenuItem.Name = "recordingToolStripMenuItem";
            this.recordingToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.recordingToolStripMenuItem.Text = "Recording";
            this.recordingToolStripMenuItem.Click += new System.EventHandler(this.recordingToolStripMenuItem_Click);
            // 
            // imageDetectionToolStripMenuItem
            // 
            this.imageDetectionToolStripMenuItem.AnalyticsEnabled = true;
            this.imageDetectionToolStripMenuItem.Name = "imageDetectionToolStripMenuItem";
            this.imageDetectionToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.imageDetectionToolStripMenuItem.Text = "Detection";
            this.imageDetectionToolStripMenuItem.Click += new System.EventHandler(this.imageDetectionToolStripMenuItem_Click);
            // 
            // compilerToolStripMenuItem
            // 
            this.compilerToolStripMenuItem.AnalyticsEnabled = true;
            this.compilerToolStripMenuItem.Name = "compilerToolStripMenuItem";
            this.compilerToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.compilerToolStripMenuItem.Text = "Compiler";
            this.compilerToolStripMenuItem.Click += new System.EventHandler(this.compilerToolStripMenuItem_Click);
            // 
            // testRunnerToolStripMenuItem
            // 
            this.testRunnerToolStripMenuItem.AnalyticsEnabled = true;
            this.testRunnerToolStripMenuItem.Name = "testRunnerToolStripMenuItem";
            this.testRunnerToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.testRunnerToolStripMenuItem.Text = "Test Runner";
            this.testRunnerToolStripMenuItem.Click += new System.EventHandler(this.testRunnerToolStripMenuItem_Click);
            // 
            // hierarchyToolStripMenuItem
            // 
            this.hierarchyToolStripMenuItem.AnalyticsEnabled = true;
            this.hierarchyToolStripMenuItem.Name = "hierarchyToolStripMenuItem";
            this.hierarchyToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.hierarchyToolStripMenuItem.Text = "Hierarchy";
            this.hierarchyToolStripMenuItem.Click += new System.EventHandler(this.hierarchyToolStripMenuItem_Click);
            // 
            // imagePreviewToolStripMenuItem
            // 
            this.imagePreviewToolStripMenuItem.AnalyticsEnabled = true;
            this.imagePreviewToolStripMenuItem.Name = "imagePreviewToolStripMenuItem";
            this.imagePreviewToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.imagePreviewToolStripMenuItem.Text = "Image Preview";
            this.imagePreviewToolStripMenuItem.Click += new System.EventHandler(this.imagePreviewToolStripMenuItem_Click);
            // 
            // assetsToolStripMenuItem
            // 
            this.assetsToolStripMenuItem.AnalyticsEnabled = true;
            this.assetsToolStripMenuItem.Name = "assetsToolStripMenuItem";
            this.assetsToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.assetsToolStripMenuItem.Text = "Assets";
            this.assetsToolStripMenuItem.Click += new System.EventHandler(this.assetsToolStripMenuItem_Click);
            // 
            // profilerToolStripMenuItem
            // 
            this.profilerToolStripMenuItem.AnalyticsEnabled = true;
            this.profilerToolStripMenuItem.Name = "profilerToolStripMenuItem";
            this.profilerToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.profilerToolStripMenuItem.Text = "Profiler";
            this.profilerToolStripMenuItem.Click += new System.EventHandler(this.profilerToolStripMenuItem_Click);
            // 
            // inspectorToolStripMenuItem
            // 
            this.inspectorToolStripMenuItem.AnalyticsEnabled = true;
            this.inspectorToolStripMenuItem.Name = "inspectorToolStripMenuItem";
            this.inspectorToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.inspectorToolStripMenuItem.Text = "Inspector";
            this.inspectorToolStripMenuItem.Click += new System.EventHandler(this.inspectorToolStripMenuItem_Click);
            // 
            // consoleToolStripMenuItem
            // 
            this.consoleToolStripMenuItem.AnalyticsEnabled = true;
            this.consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
            this.consoleToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.consoleToolStripMenuItem.Text = "Console";
            this.consoleToolStripMenuItem.Click += new System.EventHandler(this.consoleToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.AnalyticsEnabled = true;
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetDefaultLayoutToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.AnalyticsEnabled = true;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // resetDefaultLayoutToolStripMenuItem
            // 
            this.resetDefaultLayoutToolStripMenuItem.Name = "resetDefaultLayoutToolStripMenuItem";
            this.resetDefaultLayoutToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.resetDefaultLayoutToolStripMenuItem.Text = "Reset Default Layout";
            this.resetDefaultLayoutToolStripMenuItem.Click += new System.EventHandler(this.resetDefaultLayoutToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 494);
            this.Controls.Add(this.m_DockPanel);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.Text = "Mouse Robot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private WeifenLuo.WinFormsUI.Docking.DockPanel m_DockPanel;
        private WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender visualStudioToolStripExtender;
        private System.Windows.Forms.StatusStrip statusStrip;
        private WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme vS2015DarkTheme1;
        private WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme vS2015BlueTheme1;
        private WeifenLuo.WinFormsUI.Docking.VS2015LightTheme vS2015LightTheme1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.MenuStrip menuStrip;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem fileToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem openRecordingToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem saveAllRecordingsToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem saveRecordingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator exitToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem exitToolStripMenuItem1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem editToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem duplicateToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem deleteToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem helpToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem aboutToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem windowToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem hierarchyToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem optionsToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem darkThemeToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem blueThemeToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem whiteThemeToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem commandsToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripButton playButton;
        private RobotEditor.CustomControls.TrackedToolStripComboBox actionOnRec;
        private RobotEditor.CustomControls.TrackedToolStripButton recordButton;
        private RobotEditor.CustomControls.TrackedToolStripComboBox actionOnPlay;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem imagePreviewToolStripMenuItem;
        private System.Windows.Forms.BindingSource bindingSource1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem assetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private RobotEditor.CustomControls.TrackedToolStripButton visualizationButton;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem profilerToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem recordingToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem imageDetectionToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem inspectorToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem openProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem consoleToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem testRunnerToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel leftLabel;
        private System.Windows.Forms.ToolStripStatusLabel rightLabel;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem newToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem newProjectToolStripMenuItem1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem newTestFixtureToolStripMenuItem1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem newRecordingToolStripMenuItem1;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem compilerToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripMenuItem addScriptToolStripMenuItem;
        private RobotEditor.CustomControls.TrackedToolStripButton textDetectionButton;
        private System.Windows.Forms.ToolStripMenuItem resetDefaultLayoutToolStripMenuItem;
    }
}