using Robot.Utils.Win32;
using RobotEditor.Editor;
using RobotEditor.Abstractions;
using RobotRuntime;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using RobotEditor.Windows;
using Robot.Settings;
using RobotRuntime.Settings;
using RobotRuntime.Logging;
using Robot.Scripts;
using Unity;
using RobotRuntime.Tests;
using System.Collections.Generic;
using System.Linq;

namespace RobotEditor
{
    public partial class MainForm : Form, IMainForm
    {
        private DockContent[] m_Windows;
        private ThemeBase m_CurrentTheme;
        private readonly VisualStudioToolStripExtender.VsVersion m_VsVersion = VisualStudioToolStripExtender.VsVersion.Vs2015;

        private FormWindowState m_DefaultWindowState;

        private IList<TestFixtureWindow> TestFixtureWindows = new List<TestFixtureWindow>();

        private IHierarchyWindow m_HierarchyWindow;
        private IPropertiesWindow m_PropertiesWindow;
        private IScreenPreviewWindow m_ScreenPreviewWindow;
        private IAssetsWindow m_AssetsWindow;
        private IProfilerWindow m_ProfilerWindow;
        private IInspectorWindow m_InspectorWindow;
        private IConsoleWindow m_ConsoleWindow;

        private IMouseRobot MouseRobot;
        private IScreenPaintForm ScreenPaintForm;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ISettingsManager SettingsManager;
        private IScriptManager ScriptManager;
        private IAssetManager AssetManager;
        private IScreenStateThread ScreenStateThread;
        private IInputCallbacks InputCallbacks;
        private IStatusManager StatusManager;
        private ITestFixtureManager TestFixtureManager;

        private IProjectSelectionDialog ProjectSelectionDialog;
        private new IUnityContainer Container;
        public MainForm(IUnityContainer Container, IMouseRobot MouseRobot, IScreenPaintForm ScreenPaintForm, IFeatureDetectionThread FeatureDetectionThread, ISettingsManager SettingsManager,
            IScriptManager ScriptManager, IAssetManager AssetManager, IHierarchyWindow HierarchyWindow, IPropertiesWindow PropertiesWindow, IScreenPreviewWindow ScreenPreviewWindow,
            IAssetsWindow AssetsWindow, IProfilerWindow ProfilerWindow, IInspectorWindow InspectorWindow, IScreenStateThread ScreenStateThread, IInputCallbacks InputCallbacks,
            IProjectSelectionDialog ProjectSelectionDialog, IConsoleWindow ConsoleWindow, IStatusManager StatusManager, ITestFixtureManager TestFixtureManager)
        {
            this.Container = Container;

            this.MouseRobot = MouseRobot;
            this.ScreenPaintForm = ScreenPaintForm;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.SettingsManager = SettingsManager;
            this.ScriptManager = ScriptManager;
            this.AssetManager = AssetManager;
            this.ScreenStateThread = ScreenStateThread;
            this.InputCallbacks = InputCallbacks;
            this.StatusManager = StatusManager;
            this.TestFixtureManager = TestFixtureManager;
            
            this.m_HierarchyWindow = HierarchyWindow;
            this.m_PropertiesWindow = PropertiesWindow;
            this.m_ScreenPreviewWindow = ScreenPreviewWindow;
            this.m_AssetsWindow = AssetsWindow;
            this.m_ProfilerWindow = ProfilerWindow;
            this.m_InspectorWindow = InspectorWindow;
            this.m_ConsoleWindow = ConsoleWindow;

            this.ProjectSelectionDialog = ProjectSelectionDialog;

            MouseRobot.AsyncOperationOnUI = AsyncOperationManager.CreateOperation(null);

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            this.WindowState = FormWindowState.Maximized;

            //ShowSplashScreen(2000);

            ((Form)ScreenPaintForm).Owner = this;

            PutLayoutWindowsInArray();
            SetWindowTheme(this.vS2015DarkTheme1, emptyLayout: true);

            DockLayout.Windows = m_Windows;
            DockLayout.Restore(m_DockPanel);

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();

            actionOnRec.SelectedIndex = 2;
            actionOnPlay.SelectedIndex = 2;

            InputCallbacks.InputEvent += OnInputEvent;

            m_AssetsWindow.AssetSelected += OnAssetSelected;
            m_HierarchyWindow.OnSelectionChanged += ShowSelectedObjectInInspector;

            MouseRobot.RecordingStateChanged += OnRecordingStateChanged;
            MouseRobot.PlayingStateChanged += OnPlayingStateChanged;
            MouseRobot.VisualizationStateChanged += OnVisualizationStateChanged;

            StatusManager.StatusUpdated += OnStatusUpdated;

            TestFixtureManager.FixtureAdded += OnFixtureAdded;

            this.Activated += (x, y) => AssetManager.Refresh();

            ((Form)ScreenPaintForm).Show();
        }

        private void OnFixtureAdded(TestFixture fixture)
        {
            // Skips closed forms
            TestFixtureWindows = TestFixtureWindows.Where(form => form.Created && !form.Disposing).ToList();
            var window = Container.Resolve<TestFixtureWindow>();

            ShowWindowPreferablyUponAnotherTestFixtureWindowAsTab(window);

            SetTestFixtureWindowTheme(window, m_VsVersion, m_CurrentTheme);

            window.DisplayTestFixture(fixture);
            window.OnSelectionChanged += ShowSelectedObjectInInspector;
        }

        private void ShowWindowPreferablyUponAnotherTestFixtureWindowAsTab(TestFixtureWindow window)
        {
            if (TestFixtureWindows.Count > 0)
            {
                var parent = TestFixtureWindows[0];
                window.Show(parent.Pane, null); // Show as tab on another test fixture window
            }
            else
            {
                var firstWindow = m_Windows[0];
                window.Show(firstWindow.Pane, DockAlignment.Right, 0.5); // Dock to the right to first window in list ([0] is HierarhcyWindows)
            }
            TestFixtureWindows.Add(window);
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
            if (MouseRobot.IsVisualizationOn)
                FeatureDetectionThread.StartNewImageSearch(m_AssetsWindow.GetSelectedAsset().Path);
        }

        private void ShowSelectedObjectInInspector(BaseScriptManager BaseScriptManager, object obj)
        {
            m_InspectorWindow.ShowObject(obj, BaseScriptManager);
        }

        private void OnStatusUpdated(Status status)
        {
            if (!this.Created || !statusStrip.Created)
                return;

            this.Invoke(new MethodInvoker(() =>
            {
                statusStrip.Items[0].Text = status.EditorStatus;
                statusStrip.Items[1].Text = status.CurrentOperation;
                statusStrip.BackColor = status.Color;
            }));
        }

        private void PutLayoutWindowsInArray()
        {
            m_Windows = new DockContent[]
            {
                (DockContent)m_HierarchyWindow,
                (DockContent)m_PropertiesWindow,
                (DockContent)m_ScreenPreviewWindow,
                (DockContent)m_AssetsWindow,
                (DockContent)m_ProfilerWindow,
                (DockContent)m_InspectorWindow,
                (DockContent)m_ConsoleWindow,
            };
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

            EnableVSDesignForToolstrips(m_VsVersion, theme);

            if (m_DockPanel.Theme.ColorPalette != null)
                statusStrip.BackColor = m_DockPanel.Theme.ColorPalette.MainWindowStatusBarDefault.Background;

            if (!emptyLayout)
                DockLayout.Restore(m_DockPanel);
        }

        private void EnableVSDesignForToolstrips(VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            visualStudioToolStripExtender.SetStyle(menuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(toolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(statusStrip, version, theme);

            visualStudioToolStripExtender.SetStyle(((Form)m_HierarchyWindow).ContextMenuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(((Form)m_AssetsWindow).ContextMenuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(((Form)m_PropertiesWindow).ContextMenuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(((Form)m_ConsoleWindow).ContextMenuStrip, version, theme);

            visualStudioToolStripExtender.SetStyle(m_HierarchyWindow.ToolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(m_ProfilerWindow.ToolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(m_ConsoleWindow.ToolStrip, version, theme);
            m_ProfilerWindow.FrameSlider.BackColor = theme.ColorPalette.CommandBarToolbarDefault.Background;

            foreach (var window in TestFixtureWindows)
                SetTestFixtureWindowTheme(window, version, theme);
        }

        private void SetTestFixtureWindowTheme(TestFixtureWindow window, VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            if (window.ContextMenuStrip != null)
                visualStudioToolStripExtender.SetStyle(window.ContextMenuStrip, version, theme);
            if (window.ToolStrip != null)
                visualStudioToolStripExtender.SetStyle(window.ToolStrip, version, theme);
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
            if (((Form)m_HierarchyWindow).ContainsFocus)
                m_HierarchyWindow.SaveAllScripts();

            // Removes closed forms
            TestFixtureWindows = TestFixtureWindows.Where(form => form.Created && !form.Disposing).ToList();

            TestFixtureWindows.FirstOrDefault(w => w.ContainsFocus)?.SaveTestFixture();
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((Form)m_HierarchyWindow).ContainsFocus)
                m_HierarchyWindow.SaveSelectedScriptWithDialog(ScriptManager.ActiveScript, true);

            // Removes closed forms
            TestFixtureWindows = TestFixtureWindows.Where(form => form.Created && !form.Disposing).ToList();

            TestFixtureWindows.FirstOrDefault(w => w.ContainsFocus)?.SaveFixtureWithDialog(true);
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_HierarchyWindow.newScriptToolStripMenuItem1_Click(sender, e);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_HierarchyWindow.deleteToolStripMenuItem1_Click(sender, e);
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_HierarchyWindow.duplicateToolStripMenuItem1_Click(sender, e);
        }
        #endregion

        #region Menu Items (General)

        private void importAssetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = string.Format("Files|*.*|Script File|*.{0}|Image File|*.{1}|Timeline File|*.{2}",
                FileExtensions.Script, FileExtensions.Image, FileExtensions.Test);

            openDialog.Title = "Select files to import.";
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string unimportedPaths = "";
                foreach (var path in openDialog.FileNames)
                {
                    var folder = Paths.GetFolderFromExtension(path);
                    if (folder != "")
                    {
                        var newPath = folder + "\\" + Paths.GetNameWithExtension(path);
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

                AssetManager.Refresh();
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
        #endregion

        #region Menu Items (Windows)

        private void hierarchyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((HierarchyWindow)m_HierarchyWindow).Show(m_DockPanel);
        }

        private void imagePreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ScreenPreviewWindow)m_ScreenPreviewWindow).Show(m_DockPanel);
        }

        private void assetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((AssetsWindow)m_AssetsWindow).Show(m_DockPanel);
        }

        private void profilerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ProfilerWindow)m_ProfilerWindow).Show(m_DockPanel);
        }

        private void inspectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((InspectorWindow)m_InspectorWindow).Show(m_DockPanel);
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ConsoleWindow)m_ConsoleWindow).Show(m_DockPanel);
        }

        #endregion

        #region Menu Items (Windows -> Settings)

        private void recordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((Form)m_PropertiesWindow).Show(m_DockPanel);
            m_PropertiesWindow.ShowSettings(SettingsManager.GetSettings<RecordingSettings>());
        }

        private void imageDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((Form)m_PropertiesWindow).Show(m_DockPanel);
            m_PropertiesWindow.ShowSettings(SettingsManager.GetSettings<FeatureDetectionSettings>());
        }

        #endregion

        #region Toolstrip item clicks
        private void playButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.IsRecording)
                return;

            MouseRobot.IsPlaying ^= true;
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.IsPlaying)
                return;

            MouseRobot.IsRecording ^= true;
        }


        private void enableVizualization_Click(object sender, EventArgs e)
        {
            MouseRobot.IsVisualizationOn ^= true;
            /*  now it is always shown. Might cause performance issues, maybe fix will come in future if it's a problem
            if (MouseRobot.IsVisualizationOn)
                ScreenDrawForm.Instace.Show();
            else
                ScreenDrawForm.Instace.Hide();*/
        }
        #endregion

        private void UpdateToolstripButtonStates()
        {
            // Change images
            playButton.Image = (MouseRobot.IsPlaying) ?
                RobotEditor.Properties.Resources.ToolButton_Stop_32 : RobotEditor.Properties.Resources.ToolButton_Play_32;

            recordButton.Image = (MouseRobot.IsRecording) ?
                RobotEditor.Properties.Resources.ToolButton_RecordStop_32 : RobotEditor.Properties.Resources.ToolButton_Record_32;

            visualizationButton.Image = (MouseRobot.IsVisualizationOn) ?
                RobotEditor.Properties.Resources.Eye_e_ICO_256 : RobotEditor.Properties.Resources.Eye_d_ICO_256;

            // Disable/Enable buttons
            playButton.Enabled = !MouseRobot.IsRecording;
            recordButton.Enabled = !MouseRobot.IsPlaying;

            // Change tooltip text
            playButton.ToolTipText = (MouseRobot.IsPlaying) ?
                RobotEditor.Properties.Settings.Default.S_Stop : RobotEditor.Properties.Settings.Default.S_Play;

            recordButton.ToolTipText = (MouseRobot.IsRecording) ?
                RobotEditor.Properties.Settings.Default.S_StopRecording : RobotEditor.Properties.Settings.Default.S_StartRecording;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ScreenStateThread.Stop();
            FeatureDetectionThread.Stop();
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DockLayout.Save(m_DockPanel);

            MouseRobot.IsRecording = false;
            MouseRobot.IsPlaying = false;
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectSelectionDialog.InitProjectWithDialog();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectSelectionDialog.InitProjectWithDialog();
        }

        private void newTestFixtureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var testFixture = TestFixtureManager.NewTestFixture();
        }
    }
}
