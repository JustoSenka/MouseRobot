using Robot;
using Robot.Graphics;
using Robot.Utils.Win32;
using RobotUI.Utils;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class MainForm : Form
    {
        private TreeViewWindow m_TreeViewWindow;
        private CommandManagerWindow m_CommandManagerWindow;
        private ScreenPreviewWindow m_ScreenPreviewWindow;

        private ThemeBase m_CurrentTheme;
        private DeserializeDockContent m_DeserializeDockContent;

        private FormWindowState m_DefaultWindowState;

        public MainForm()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            //ShowSplashScreen(2000);

            CreateWindows();
            CreateWindowDeserializer();

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();
            SetWindowTheme(this.vS2015DarkTheme1);

            actionOnRec.SelectedIndex = 0;
            actionOnPlay.SelectedIndex = 0;

            InputCallbacks.inputEvent += OnInputEvent;
        }

        private void OnInputEvent(KeyEvent obj)
        {
            if (obj.IsKeyDown() && obj.keyCode == Keys.F1)
                playButton_Click(null, null);

            if (obj.IsKeyDown() && obj.keyCode == Keys.F2)
                recordButton_Click(null, null);
        }

        private void CreateWindowDeserializer()
        {
            m_DeserializeDockContent = new DeserializeDockContent((string persistString) =>
            {
                if (persistString.Equals(typeof(TreeViewWindow).ToString()))
                    return m_TreeViewWindow;
                if (persistString.Equals(typeof(CommandManagerWindow).ToString()))
                    return m_CommandManagerWindow;
                if (persistString.Equals(typeof(ScreenPreviewWindow).ToString()))
                    return m_ScreenPreviewWindow;

                return null;
            });
        }

        private void CreateWindows()
        {
            m_TreeViewWindow = new TreeViewWindow();
            m_CommandManagerWindow = new CommandManagerWindow();
            m_ScreenPreviewWindow = new ScreenPreviewWindow();
        }

        private void SetWindowTheme(ThemeBase theme)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.temp.config");
            m_DockPanel.SaveAsXml(configFile);

            CloseAllContents();

            m_CurrentTheme = theme;
            m_DockPanel.Theme = theme;

            visualStudioToolStripExtender.SetStyle(menuStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, theme);
            visualStudioToolStripExtender.SetStyle(toolStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, theme);
            visualStudioToolStripExtender.SetStyle(statusStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, theme);

            if (m_DockPanel.Theme.ColorPalette != null)
                statusStrip.BackColor = m_DockPanel.Theme.ColorPalette.MainWindowStatusBarDefault.Background;

            if (File.Exists(configFile))
                m_DockPanel.LoadFromXml(configFile, m_DeserializeDockContent);
        }

        private void EnableVSRenderer(VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            visualStudioToolStripExtender.SetStyle(menuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(toolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(statusStrip, version, theme);
        }

        private void CloseAllContents()
        {
            m_TreeViewWindow.DockPanel = null;
            m_CommandManagerWindow.DockPanel = null;
            m_ScreenPreviewWindow.DockPanel = null;

            CloseAllDocuments();

            foreach (var window in m_DockPanel.FloatWindows.ToList())
                window.Dispose();

            System.Diagnostics.Debug.Assert(m_DockPanel.Panes.Count == 0);
            System.Diagnostics.Debug.Assert(m_DockPanel.Contents.Count == 0);
            System.Diagnostics.Debug.Assert(m_DockPanel.FloatWindows.Count == 0);
        }

        private void CloseAllDocuments()
        {
            foreach (IDockContent document in m_DockPanel.DocumentsToArray())
            {
                document.DockHandler.DockPanel = null;
                document.DockHandler.Close();
            }
        }

        private void ShowSplashScreen(int millis)
        {
            var splash = new SplashScreen();

            splash.Visible = true;
            splash.TopMost = true;

            Timer timer = new Timer();
            timer.Tick += (sender, e) =>
            {
                splash.Visible = false;
                splash.Dispose();
                timer.Enabled = false;
            };
            timer.Interval = millis;
            timer.Enabled = true;
        }

        #region Menu Items (ScriptManager)
        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.OpenScript(m_TreeViewWindow.treeView);
        }

        private void saveAllScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveAllScripts(m_TreeViewWindow.treeView);
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveScript(ScriptManager.Instance.ActiveScript, m_TreeViewWindow.treeView, true);
        }

        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SetSelectedScriptActive(m_TreeViewWindow.treeView);
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.NewScript(m_TreeViewWindow.treeView);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.ShowSelectedTreeViewItemInExplorer(m_TreeViewWindow.treeView);
        }
        #endregion

        #region Menu Items (General)
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog(this);
        }

        private void darkThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_CurrentTheme != vS2015DarkTheme1)
                SetWindowTheme(this.vS2015DarkTheme1);
        }

        private void blueThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_CurrentTheme != vS2015BlueTheme1)
                SetWindowTheme(this.vS2015BlueTheme1);
        }

        private void lightThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_CurrentTheme != vS2015LightTheme1)
                SetWindowTheme(this.vS2015LightTheme1);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void hierarchyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_TreeViewWindow.Show(m_DockPanel);
        }

        private void commandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_CommandManagerWindow.Show(m_DockPanel);
        }

        private void imagePreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_ScreenPreviewWindow.Show(m_DockPanel);
        }
        #endregion

        #region Toolstrip item clicks
        private void playButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.Instance.IsRecording)
                return;

            MouseRobot.Instance.IsPlaying ^= true;
            UpdateToolstripButtonStates();

            if (MouseRobot.Instance.IsPlaying && actionOnPlay.SelectedIndex == 0)
            {
                m_DefaultWindowState = this.WindowState;
                this.WindowState = FormWindowState.Minimized;
            }

            if (!MouseRobot.Instance.IsPlaying)
                this.WindowState = m_DefaultWindowState;
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.Instance.IsPlaying)
                return;

            MouseRobot.Instance.IsRecording ^= true;
            UpdateToolstripButtonStates();

            if (MouseRobot.Instance.IsRecording && actionOnRec.SelectedIndex == 0)
            {
                m_DefaultWindowState = this.WindowState;
                this.WindowState = FormWindowState.Minimized;
            }

            if (!MouseRobot.Instance.IsRecording)
                this.WindowState = m_DefaultWindowState;
        }
        #endregion

        private void UpdateToolstripButtonStates()
        {
            // Change images
            playButton.Image = (MouseRobot.Instance.IsPlaying) ?
                Robot.Properties.Resources.ToolButton_Stop_32 : Robot.Properties.Resources.ToolButton_Play_32;

            recordButton.Image = (MouseRobot.Instance.IsRecording) ?
                Robot.Properties.Resources.ToolButton_RecordStop_32 : Robot.Properties.Resources.ToolButton_Record_32;

            // Disable/Enable buttons
            playButton.Enabled = !MouseRobot.Instance.IsRecording;
            recordButton.Enabled = !MouseRobot.Instance.IsPlaying;

            // Change tooltip text
            playButton.ToolTipText = (MouseRobot.Instance.IsPlaying) ?
                Robot.Properties.Settings.Default.S_Stop : Robot.Properties.Settings.Default.S_Play;

            recordButton.ToolTipText = (MouseRobot.Instance.IsRecording) ?
                Robot.Properties.Settings.Default.S_StopRecording : Robot.Properties.Settings.Default.S_StartRecording;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ScreenStateThread.Instace.Stop();
            Application.Exit();
        }
    }
}
