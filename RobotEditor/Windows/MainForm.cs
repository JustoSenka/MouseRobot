using Robot;
using Robot.Graphics;
using Robot.Utils.Win32;
using RobotEditor.Editor;
using RobotEditor.Utils;
using RobotRuntime;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class MainForm : Form
    {
        private DockContent[] m_Windows;

        // Solve this problem somehow
        private TreeViewWindow m_TreeViewWindow;
        private CommandManagerWindow m_CommandManagerWindow;
        private ScreenPreviewWindow m_ScreenPreviewWindow;
        private AssetsWindow m_AssetsWindow;

        private ThemeBase m_CurrentTheme;

        private FormWindowState m_DefaultWindowState;

        public MainForm()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            //ShowSplashScreen(2000);

            this.WindowState = FormWindowState.Maximized;
            SetWindowTheme(this.vS2015DarkTheme1, emptyLayout: true);

            CreateWindows();
            DockLayout.Restore(m_DockPanel);

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();

            actionOnRec.SelectedIndex = 2;
            actionOnPlay.SelectedIndex = 2;

            InputCallbacks.inputEvent += OnInputEvent;

            m_AssetsWindow.AssetSelected += OnAssetSelected;

            MouseRobot.Instance.RecordingStateChanged += OnRecordingStateChanged;
            MouseRobot.Instance.PlayingStateChanged += OnPlayingStateChanged;
        }

        private void OnPlayingStateChanged(bool isPlaying)
        {
            // Since Playing State can be changed from ScriptThread, we need to make sure we run this on UI thread
            this.BeginInvoke(new MethodInvoker(delegate
            {
                if (isPlaying && actionOnPlay.SelectedIndex == 0)
                {
                    m_DefaultWindowState = this.WindowState;
                    this.WindowState = FormWindowState.Minimized;
                }

                if (!isPlaying && actionOnPlay.SelectedIndex == 0)
                    this.WindowState = m_DefaultWindowState;

                UpdateToolstripButtonStates();
            }));
        }

        private void OnRecordingStateChanged(bool isRecording)
        {
            if (isRecording && actionOnRec.SelectedIndex == 0)
            {
                m_DefaultWindowState = this.WindowState;
                this.WindowState = FormWindowState.Minimized;
            }

            if (!isRecording && actionOnRec.SelectedIndex == 0)
                this.WindowState = m_DefaultWindowState;

            UpdateToolstripButtonStates();
        }

        private void OnInputEvent(KeyEvent obj)
        {
            if (obj.IsKeyDown() && obj.keyCode == Keys.F1)
                playButton_Click(null, null);

            if (obj.IsKeyDown() && obj.keyCode == Keys.F2)
                recordButton_Click(null, null);
        }

        private void OnAssetSelected()
        {
            m_ScreenPreviewWindow.Preview(m_AssetsWindow.GetSelectedAsset());
        }

        private void CreateWindows()
        {
            m_TreeViewWindow = new TreeViewWindow();
            m_CommandManagerWindow = new CommandManagerWindow();
            m_ScreenPreviewWindow = new ScreenPreviewWindow();
            m_AssetsWindow = new AssetsWindow();

            m_Windows = new DockContent[]
            {
                m_TreeViewWindow,
                m_CommandManagerWindow,
                m_ScreenPreviewWindow,
                m_AssetsWindow,
            };

            DockLayout.Windows = m_Windows;
        }

        private void SetWindowTheme(ThemeBase theme, bool emptyLayout = false)
        {
            if (!emptyLayout)
            {
                DockLayout.Save(m_DockPanel);
                DockLayout.CloseAllContents(m_DockPanel);
            }

            m_CurrentTheme = theme;
            m_DockPanel.Theme = theme;

            EnableVSRenderer(VisualStudioToolStripExtender.VsVersion.Vs2015, theme);

            if (m_DockPanel.Theme.ColorPalette != null)
                statusStrip.BackColor = m_DockPanel.Theme.ColorPalette.MainWindowStatusBarDefault.Background;

            if (!emptyLayout)
                DockLayout.Restore(m_DockPanel);
        }

        private void EnableVSRenderer(VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            visualStudioToolStripExtender.SetStyle(menuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(toolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(statusStrip, version, theme);
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

        private void assetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_AssetsWindow.Show(m_DockPanel);
        }
        #endregion

        #region Toolstrip item clicks
        private void playButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.Instance.IsRecording)
                return;

            MouseRobot.Instance.IsPlaying ^= true;
            //UpdateToolstripButtonStates();
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.Instance.IsPlaying)
                return;

            MouseRobot.Instance.IsRecording ^= true;
            //UpdateToolstripButtonStates();
        }
        #endregion

        private void UpdateToolstripButtonStates()
        {
            // Change images
            playButton.Image = (MouseRobot.Instance.IsPlaying) ?
                RobotEditor.Properties.Resources.ToolButton_Stop_32 : RobotEditor.Properties.Resources.ToolButton_Play_32;

            recordButton.Image = (MouseRobot.Instance.IsRecording) ?
                RobotEditor.Properties.Resources.ToolButton_RecordStop_32 : RobotEditor.Properties.Resources.ToolButton_Record_32;

            // Disable/Enable buttons
            playButton.Enabled = !MouseRobot.Instance.IsRecording;
            recordButton.Enabled = !MouseRobot.Instance.IsPlaying;

            // Change tooltip text
            playButton.ToolTipText = (MouseRobot.Instance.IsPlaying) ?
                RobotEditor.Properties.Settings.Default.S_Stop : RobotEditor.Properties.Settings.Default.S_Play;

            recordButton.ToolTipText = (MouseRobot.Instance.IsRecording) ?
                RobotEditor.Properties.Settings.Default.S_StopRecording : RobotEditor.Properties.Settings.Default.S_StartRecording;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ScreenStateThread.Instace.Stop();
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DockLayout.Save(m_DockPanel);
        }
    }
}
