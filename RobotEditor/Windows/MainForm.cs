using Robot;
using Robot.Abstractions;
using Robot.Settings;
using Robot.Tests;
using RobotEditor.Abstractions;
using RobotEditor.Editor;
using RobotEditor.Windows;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Logging;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Unity;
using Unity.Lifetime;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    [RegisterTypeToContainer(typeof(IMainForm), typeof(ContainerControlledLifetimeManager))]
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
        private ITestRunnerWindow m_TestRunnerWindow;

        private IMouseRobot MouseRobot;
        private IScreenPaintForm ScreenPaintForm;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ISettingsManager SettingsManager;
        private IHierarchyManager RecordingManager;
        private IAssetManager AssetManager;
        private IScreenStateThread ScreenStateThread;
        private IStatusManager StatusManager;
        private ITestFixtureManager TestFixtureManager;
        private ITestRunner TestRunner;
        private IProjectManager ProjectManager;

        private IScriptTemplates ScriptTemplates;
        private IHotkeyCallbacks HotkeyCallbacks;

        private IProjectSelectionDialog ProjectSelectionDialog;
        private new IUnityContainer Container;
        public MainForm(IUnityContainer Container, IMouseRobot MouseRobot, IScreenPaintForm ScreenPaintForm, IFeatureDetectionThread FeatureDetectionThread, ISettingsManager SettingsManager,
            IHierarchyManager RecordingManager, IAssetManager AssetManager, IHierarchyWindow HierarchyWindow, IPropertiesWindow PropertiesWindow, IScreenPreviewWindow ScreenPreviewWindow,
            IAssetsWindow AssetsWindow, IProfilerWindow ProfilerWindow, IInspectorWindow InspectorWindow, IScreenStateThread ScreenStateThread,
            IProjectSelectionDialog ProjectSelectionDialog, IConsoleWindow ConsoleWindow, IStatusManager StatusManager, ITestFixtureManager TestFixtureManager,
            ITestRunnerWindow TestRunnerWindow, ITestRunner TestRunner, IProjectManager ProjectManager, IScriptTemplates ScriptTemplates, IHotkeyCallbacks HotkeyCallbacks)
        {
            this.Container = Container;

            this.MouseRobot = MouseRobot;
            this.ScreenPaintForm = ScreenPaintForm;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.SettingsManager = SettingsManager;
            this.RecordingManager = RecordingManager;
            this.AssetManager = AssetManager;
            this.ScreenStateThread = ScreenStateThread;
            this.StatusManager = StatusManager;
            this.TestFixtureManager = TestFixtureManager;
            this.TestRunner = TestRunner;
            this.ProjectManager = ProjectManager;

            this.m_HierarchyWindow = HierarchyWindow;
            this.m_PropertiesWindow = PropertiesWindow;
            this.m_ScreenPreviewWindow = ScreenPreviewWindow;
            this.m_AssetsWindow = AssetsWindow;
            this.m_ProfilerWindow = ProfilerWindow;
            this.m_InspectorWindow = InspectorWindow;
            this.m_ConsoleWindow = ConsoleWindow;
            this.m_TestRunnerWindow = TestRunnerWindow;

            this.ScriptTemplates = ScriptTemplates;
            this.HotkeyCallbacks = HotkeyCallbacks;

            this.ProjectSelectionDialog = ProjectSelectionDialog;

            MouseRobot.AsyncOperationOnUI = AsyncOperationManager.CreateOperation(null);

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            this.WindowState = FormWindowState.Maximized;

            //ShowSplashScreen(2000);

            // ((Form)ScreenPaintForm).Owner = this;

            PutLayoutWindowsInArray();
            SetWindowTheme(this.vS2015DarkTheme1, emptyLayout: true);

            DockLayout.Windows = m_Windows;
            DockLayout.Restore(m_DockPanel);

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();

            actionOnRec.SelectedIndex = 2;
            actionOnPlay.SelectedIndex = 2;

            RegisterFormHotkeys(HotkeyCallbacks);

            m_AssetsWindow.AssetSelected += OnAssetSelected;
            m_HierarchyWindow.OnSelectionChanged += ShowSelectedObjectInInspector;

            MouseRobot.RecordingStateChanged += OnRecordingStateChanged;
            MouseRobot.PlayingStateChanged += OnPlayingStateChanged;
            MouseRobot.VisualizationStateChanged += OnVisualizationStateChanged;
            MouseRobot.TextDetectionStateChanged += OnVisualizationStateChanged;

            StatusManager.StatusUpdated += OnStatusUpdated;

            TestFixtureManager.FixtureAdded += OnFixtureAdded;
            TestFixtureManager.FixtureRemoved += OnFixtureRemoved;

            this.Activated += OnFormActivated;

            menuStrip.HandleCreated += (s, e) => m_AssetsWindow.AddMenuItemsForScriptTemplates(menuStrip, "addScriptToolStripMenuItem");

            ((Form)ScreenPaintForm).Show();
        }

        protected override void WndProc(ref Message m)
        {
            HotkeyCallbacks.ProcessCallback(m);
            base.WndProc(ref m);
        }

        private void RegisterFormHotkeys(IHotkeyCallbacks HotkeyCallbacks)
        {
            HotkeyCallbacks.RegisterForm(this);
            HotkeyCallbacks.Register(Keys.F1, () => playButton_Click(null, null));
            HotkeyCallbacks.Register(Keys.F2, () => recordButton_Click(null, null));
        }

        private void OnFormActivated(object sender, EventArgs e)
        {
            AssetManager.Refresh();

            this.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                this.Text = ProjectManager.ProjectName + " - " + Paths.AppName;
            }));
        }

        private void OnFixtureAdded(TestFixture fixture)
        {
            TestFixtureWindows = GetNotDestroyedWindows(TestFixtureWindows).ToList();
            var window = Container.Resolve<TestFixtureWindow>();

            ShowWindowPreferablyUponAnotherTestFixtureWindowAsTab(window);

            SetTestFixtureWindowTheme((TestFixtureWindow)window, m_VsVersion, m_CurrentTheme);

            window.DisplayTestFixture(fixture);
            window.OnSelectionChanged += ShowSelectedObjectInInspector;
        }

        private void OnFixtureRemoved(TestFixture fixture)
        {
            TestFixtureWindows = GetNotDestroyedWindows(TestFixtureWindows).ToList();
        }

        private void ShowWindowPreferablyUponAnotherTestFixtureWindowAsTab(TestFixtureWindow window)
        {
            var dockContent = (DockContent)window;
            if (TestFixtureWindows.Count > 0)
            {
                var parent = (DockContent)TestFixtureWindows[0];
                dockContent.Show(parent.Pane, null); // Show as tab on another test fixture window
            }
            else
            {
                var firstWindow = m_Windows[0];
                dockContent.Show(firstWindow.Pane, DockAlignment.Right, 0.5); // Dock to the right to first window in list ([0] is HierarhcyWindow)
            }
            TestFixtureWindows.Add(window);
        }

        private void OnPlayingStateChanged(bool isPlaying)
        {
            this.BeginInvokeIfCreated(new MethodInvoker(delegate
            {
                if (isPlaying && actionOnPlay.SelectedIndex == 0)
                {
                    m_DefaultWindowState = this.WindowState;
                    this.WindowState = FormWindowState.Minimized;
                }

                if (!isPlaying && actionOnPlay.SelectedIndex == 0)
                {
                    this.WindowState = m_DefaultWindowState;
                    this.Activate();
                    this.Focus();
                }

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
            {
                this.WindowState = m_DefaultWindowState;
                this.Activate();
                this.Focus();
            }

            UpdateToolstripButtonStates();
        }

        private void OnVisualizationStateChanged(bool isVisualizationOn)
        {
            UpdateToolstripButtonStates();
        }

        private void OnAssetSelected()
        {
            var asset = m_AssetsWindow.GetSelectedAsset();
            if (asset == null)
                return;

            if (asset.Importer.GetType() != typeof(ImageImporter))
                return;

            asset.Importer.Load<object>(); // Force loading before check
            if (asset.Importer.LoadingFailed)
                return;

            m_ScreenPreviewWindow.Preview(asset);
            if (MouseRobot.IsVisualizationOn && asset.HoldsTypeOf(typeof(Bitmap)))
            {
                var settings = SettingsManager.GetSettings<FeatureDetectionSettings>();
                FeatureDetectionThread.StartNewImageSearch(m_AssetsWindow.GetSelectedAsset().Importer.Load<Bitmap>(), settings.DetectionMode);
            }
        }

        private void ShowSelectedObjectInInspector(IBaseHierarchyManager BaseHierarchyManager, object obj)
        {
            m_InspectorWindow.ShowObject(obj, BaseHierarchyManager);
        }

        private void OnStatusUpdated(Status status)
        {
            if (!this.Created || !statusStrip.Created || this.Disposing || this.IsDisposed ||
                !statusStrip.Created || statusStrip.IsDisposed || statusStrip.Disposing)
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
                (DockContent)m_TestRunnerWindow,
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
            visualStudioToolStripExtender.SetStyle(((Form)m_TestRunnerWindow).ContextMenuStrip, version, theme);

            visualStudioToolStripExtender.SetStyle(m_HierarchyWindow.ToolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(m_ProfilerWindow.ToolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(m_ConsoleWindow.ToolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(m_TestRunnerWindow.ToolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(m_AssetsWindow.ToolStrip, version, theme);
            m_ProfilerWindow.FrameSlider.BackColor = theme.ColorPalette.CommandBarToolbarDefault.Background;

            foreach (var window in TestFixtureWindows)
                SetTestFixtureWindowTheme((TestFixtureWindow)window, version, theme);
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



        #region Menu Items (RecordingManager)

        private void saveAllRecordingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((Form)m_HierarchyWindow).ContainsFocus)
                m_HierarchyWindow.SaveAllRecordings();

            TestFixtureWindows = GetNotDestroyedWindows(TestFixtureWindows).ToList();

            TestFixtureWindows.FirstOrDefault(w => ((Form)w).ContainsFocus)?.SaveTestFixture();
        }

        private void saveRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((Form)m_HierarchyWindow).ContainsFocus)
                m_HierarchyWindow.SaveSelectedRecordingWithDialog(RecordingManager.ActiveRecording, true);

            TestFixtureWindows = GetNotDestroyedWindows(TestFixtureWindows).ToList();

            TestFixtureWindows.FirstOrDefault(w => ((Form)w).ContainsFocus)?.SaveFixtureWithDialog(true);
        }

        private IEnumerable<TestFixtureWindow> GetNotDestroyedWindows(IList<TestFixtureWindow> TestFixtureWindows)
        {
            foreach (var window in TestFixtureWindows)
            {
                var form = (Form)window;
                if (form.Created && !form.Disposing && !form.IsDisposed)
                    yield return window;
            }
        }

        private void newRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_HierarchyWindow.newRecordingToolStripMenuItem1_Click(sender, e);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_HierarchyWindow.deleteToolStripMenuItem_Click(sender, e);
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
            openDialog.Filter = string.Format("Files|*.*|Recording File|*.{0}|Image File|*.{1}|Timeline File|*.{2}",
                FileExtensions.Recording, FileExtensions.Image, FileExtensions.Test);

            openDialog.Title = "Select files to import.";
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string unimportedPaths = "";
                foreach (var path in openDialog.FileNames)
                {
                    var newPath = Path.Combine(Paths.AssetsPath, Path.GetFileName(path));
                    try
                    {
                        File.Copy(path, newPath);
                    }
                    catch (IOException ex)
                    {
                        unimportedPaths += "\n " + ex.Message + " " + path;
                    }
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

        private void testRunnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((TestRunnerWindow)m_TestRunnerWindow).Show(m_DockPanel);
        }

        #endregion

        #region Menu Items (Windows -> Settings)

        private void recordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PropertiesWindow)m_PropertiesWindow).Show(m_DockPanel);
            m_PropertiesWindow.ShowSettings(SettingsManager.GetSettings<RecordingSettings>());
        }

        private void imageDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PropertiesWindow)m_PropertiesWindow).Show(m_DockPanel);
            m_PropertiesWindow.ShowSettings(SettingsManager.GetSettings<FeatureDetectionSettings>());
        }

        private void compilerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PropertiesWindow)m_PropertiesWindow).Show(m_DockPanel);
            m_PropertiesWindow.ShowSettings(SettingsManager.GetSettings<CompilerSettings>());
        }

        #endregion

        #region Toolstrip item clicks
        private void playButton_Click(object sender, EventArgs e)
        {
            if (MouseRobot.IsRecording)
                return;

            MouseRobot.IsPlaying ^= true;

            if (MouseRobot.IsPlaying)
            {
                if (RecordingManager.ActiveRecording == null)
                    MouseRobot.IsPlaying = false;
                else
                    TestRunner.StartRecording(RecordingManager.ActiveRecording.ToLightRecording());
            }
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

        private void textDetectionButton_Click(object sender, EventArgs e)
        {
            MouseRobot.IsTextDetectionOn ^= true;
        }
        #endregion

        private void UpdateToolstripButtonStates()
        {
            // Change images
            playButton.Image = (MouseRobot.IsPlaying) ?
                Properties.Resources.ToolButton_Stop_32 : Properties.Resources.ToolButton_Play_32;

            recordButton.Image = (MouseRobot.IsRecording) ?
                Properties.Resources.ToolButton_RecordStop_32 : Properties.Resources.ToolButton_Record_32;

            visualizationButton.Image = (MouseRobot.IsVisualizationOn) ?
                Properties.Resources.Eye_e_ICO_256 : Properties.Resources.Eye_d_ICO_256;

            textDetectionButton.Image = (MouseRobot.IsTextDetectionOn) ?
                Properties.Resources.Eye_e_ICO_256 : Properties.Resources.Eye_d_ICO_256;

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
            ((Form)ScreenPaintForm).Close();

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
