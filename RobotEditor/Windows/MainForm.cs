using Robot;
using Robot.Utils.Win32;
using RobotEditor.Editor;
using RobotEditor.Utils;
using RobotEditor.Windows;
using RobotRuntime;
using RobotRuntime.Graphics;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class MainForm : Form
    {
        private DockContent[] m_Windows;

        // Solve this problem somehow
        private HierarchyWindow m_HierarchyWindow;
        private CommandManagerWindow m_CommandManagerWindow;
        private ScreenPreviewWindow m_ScreenPreviewWindow;
        private AssetsWindow m_AssetsWindow;

        private ThemeBase m_CurrentTheme;

        private FormWindowState m_DefaultWindowState;

        public MainForm()
        {
            MouseRobot.Instance.AsyncOperationOnUI = AsyncOperationManager.CreateOperation(null);

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            //ShowSplashScreen(2000);
            //InvisibleForm.Instace.Owner = this;

            this.WindowState = FormWindowState.Maximized;
            SetWindowTheme(this.vS2015DarkTheme1, emptyLayout: true);

            CreateWindows();
            DockLayout.Restore(m_DockPanel);

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();

            actionOnRec.SelectedIndex = 2;
            actionOnPlay.SelectedIndex = 2;

            //InputCallbacks.inputEvent += OnInputEvent;

            m_AssetsWindow.AssetSelected += OnAssetSelected;

            MouseRobot.Instance.RecordingStateChanged += OnRecordingStateChanged;
            MouseRobot.Instance.PlayingStateChanged += OnPlayingStateChanged;
            MouseRobot.Instance.VisualizationStateChanged += OnVisualizationStateChanged;
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

        private void OnVisualizationStateChanged(bool isVisualizationOn)
        {
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
            if (MouseRobot.Instance.IsVisualizationOn)
                FeatureDetectionThread.Instace.SampleImageFromAsset = m_AssetsWindow.GetSelectedAsset().ToAssetPointer();
        }

        private void CreateWindows()
        {
            m_HierarchyWindow = new HierarchyWindow();
            m_CommandManagerWindow = new CommandManagerWindow();
            m_ScreenPreviewWindow = new ScreenPreviewWindow();
            m_AssetsWindow = new AssetsWindow();

            m_Windows = new DockContent[]
            {
                m_HierarchyWindow,
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

        private void saveAllScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveAllScripts(m_HierarchyWindow.treeView);
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveScript(ScriptManager.Instance.ActiveScript, m_HierarchyWindow.treeView, true);
        }

        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SetSelectedScriptActive(m_HierarchyWindow.treeView);
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.NewScript(m_HierarchyWindow.treeView);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(m_HierarchyWindow.treeView);
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(m_HierarchyWindow.treeView);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(m_HierarchyWindow.treeView);
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(m_HierarchyWindow.treeView);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.ShowSelectedTreeViewItemInExplorer(m_HierarchyWindow.treeView);
        }
        #endregion

        #region Menu Items (General)

        private void importAssetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = string.Format("Files|*.*|Script File|*.{0}|Image File|*.{1}|Timeline File|*.{2}",
                FileExtensions.Script, FileExtensions.Image, FileExtensions.Timeline);

            openDialog.Title = "Select files to import.";
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string unimportedPaths = "";
                foreach (var path in openDialog.FileNames)
                {
                    var folder = AssetManager.FolderFromExtension(path);
                    if (folder != "")
                    {
                        var newPath = folder + "\\" + Commons.GetNameWithExtension(path);
                        try
                        {
                            File.Copy(path, newPath);
                        }
                        catch (IOException ex)
                        {
                            unimportedPaths += "\n " + ex.Message + " " + path;
                        }
                    }
                    else
                        unimportedPaths += "\nUnsupported format: " + path;
                }

                AssetManager.Instance.Refresh();
                if (unimportedPaths != "")
                    MessageBox.Show("These files cannot be imported: " + unimportedPaths);
            }
        }

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
            m_HierarchyWindow.Show(m_DockPanel);
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
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.Instance.IsPlaying)
                return;

            MouseRobot.Instance.IsRecording ^= true;
        }


        private void enableVizualization_Click(object sender, EventArgs e)
        {
            MouseRobot.Instance.IsVisualizationOn ^= true;

            if (MouseRobot.Instance.IsVisualizationOn)
                InvisibleForm.Instace.Show();
            else
                InvisibleForm.Instace.Hide();
        }
        #endregion

        private void UpdateToolstripButtonStates()
        {
            // Change images
            playButton.Image = (MouseRobot.Instance.IsPlaying) ?
                RobotEditor.Properties.Resources.ToolButton_Stop_32 : RobotEditor.Properties.Resources.ToolButton_Play_32;

            recordButton.Image = (MouseRobot.Instance.IsRecording) ?
                RobotEditor.Properties.Resources.ToolButton_RecordStop_32 : RobotEditor.Properties.Resources.ToolButton_Record_32;

            visualizationButton.Image = (MouseRobot.Instance.IsVisualizationOn) ?
                RobotEditor.Properties.Resources.Eye_e_ICO_256 : RobotEditor.Properties.Resources.Eye_d_ICO_256;

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
            FeatureDetectionThread.Instace.Stop();
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DockLayout.Save(m_DockPanel);
        }
    }
}
