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
        private Theme m_CurrentTheme;
        private bool m_IsFirstTimeRestoringLayout = true;

        private IDictionary<Theme, ThemeBase> m_ThemeMap = new Dictionary<Theme, ThemeBase>();

        private readonly VisualStudioToolStripExtender.VsVersion m_VsVersion = VisualStudioToolStripExtender.VsVersion.Vs2015;

        private FormWindowState m_DefaultWindowState;

        private IList<TestFixtureWindow> TestFixtureWindows = new List<TestFixtureWindow>();

        readonly private IHierarchyWindow m_HierarchyWindow;
        readonly private IPropertiesWindow m_PropertiesWindow;
        readonly private IScreenPreviewWindow m_ScreenPreviewWindow;
        readonly private IAssetsWindow m_AssetsWindow;
        readonly private IProfilerWindow m_ProfilerWindow;
        readonly private IInspectorWindow m_InspectorWindow;
        readonly private IConsoleWindow m_ConsoleWindow;
        readonly private ITestRunnerWindow m_TestRunnerWindow;

        readonly private IMouseRobot MouseRobot;
        readonly private IScreenPaintForm ScreenPaintForm;
        readonly private IFeatureDetectionThread FeatureDetectionThread;
        readonly private ITextDetectionThread TextDetectionThread;
        readonly private ISettingsManager SettingsManager;
        readonly private IHierarchyManager RecordingManager;
        readonly private IAssetManager AssetManager;
        readonly private IScreenStateThread ScreenStateThread;
        readonly private IStatusManager StatusManager;
        readonly private ITestFixtureManager TestFixtureManager;
        readonly private ITestRunner TestRunner;
        readonly private IProjectManager ProjectManager;
        readonly private IAnalytics Analytics;

        readonly private IScriptTemplates ScriptTemplates;
        readonly private IHotkeyCallbacks HotkeyCallbacks;
        readonly private IFontManager FontManager;

        readonly private IProjectSelectionDialog ProjectSelectionDialog;
        readonly private new IUnityContainer Container;

        public MainForm(IUnityContainer Container, IMouseRobot MouseRobot, IScreenPaintForm ScreenPaintForm, IFeatureDetectionThread FeatureDetectionThread, ISettingsManager SettingsManager,
            IHierarchyManager RecordingManager, IAssetManager AssetManager, IHierarchyWindow HierarchyWindow, IPropertiesWindow PropertiesWindow, IScreenPreviewWindow ScreenPreviewWindow,
            IAssetsWindow AssetsWindow, IProfilerWindow ProfilerWindow, IInspectorWindow InspectorWindow, IScreenStateThread ScreenStateThread,
            IProjectSelectionDialog ProjectSelectionDialog, IConsoleWindow ConsoleWindow, IStatusManager StatusManager, ITestFixtureManager TestFixtureManager,
            ITestRunnerWindow TestRunnerWindow, ITestRunner TestRunner, IProjectManager ProjectManager, IScriptTemplates ScriptTemplates, IHotkeyCallbacks HotkeyCallbacks,
            ITextDetectionThread TextDetectionThread, IAnalytics Analytics, IFontManager FontManager)
        {
            this.Container = Container;

            this.MouseRobot = MouseRobot;
            this.ScreenPaintForm = ScreenPaintForm;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.TextDetectionThread = TextDetectionThread;
            this.SettingsManager = SettingsManager;
            this.RecordingManager = RecordingManager;
            this.AssetManager = AssetManager;
            this.ScreenStateThread = ScreenStateThread;
            this.StatusManager = StatusManager;
            this.TestFixtureManager = TestFixtureManager;
            this.TestRunner = TestRunner;
            this.ProjectManager = ProjectManager;
            this.Analytics = Analytics;

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
            this.FontManager = FontManager;

            this.ProjectSelectionDialog = ProjectSelectionDialog;

            InitializeComponent();

            MouseRobot.AsyncOperationOnUI = AsyncOperationManager.CreateOperation(null);

            AutoScaleMode = AutoScaleMode.Dpi;
            this.WindowState = FormWindowState.Maximized;

            //ShowSplashScreen(2000);
            // ((Form)ScreenPaintForm).Owner = this;

            UpdateThemeMap();
            PutLayoutWindowsInArray();
            RegisterAllFormsToTheFontManager();
            DockLayout.Windows = m_Windows;

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();

            RegisterFormHotkeys(HotkeyCallbacks);

            m_AssetsWindow.AssetSelected += OnAssetSelected;
            m_HierarchyWindow.OnSelectionChanged += ShowSelectedObjectInInspector;

            MouseRobot.RecordingStateChanged += OnRecordingStateChanged;
            MouseRobot.PlayingStateChanged += OnPlayingStateChanged;
            MouseRobot.VisualizationStateChanged += OnVisualizationStateChanged;
            MouseRobot.TextDetectionStateChanged += OnVisualizationStateChanged;

            SettingsManager.SettingsRestored += OnSettingsRestored;
            SettingsManager.SettingsModified += OnSettingsModified;
            StatusManager.StatusUpdated += OnStatusUpdated;

            TestFixtureManager.FixtureAdded += OnFixtureAdded;
            TestFixtureManager.FixtureRemoved += OnFixtureRemoved;

            this.Activated += OnFormActivated;

            menuStrip.HandleCreated += (s, e) => m_AssetsWindow.AddMenuItemsForScriptTemplates(menuStrip, "addScriptToolStripMenuItem");
            statusStrip.HandleCreated += OnStatusStripHandleCreated;

            ((Form)ScreenPaintForm).Show();

            HandleCreated += (sender, args) => OnSettingsRestored();
        }

        protected override void WndProc(ref Message m)
        {
            HotkeyCallbacks.ProcessCallback(m);
            base.WndProc(ref m);
        }

        private void UpdateThemeMap()
        {
            m_ThemeMap.Add(Theme.Dark, vS2015DarkTheme1);
            m_ThemeMap.Add(Theme.Light, vS2015LightTheme1);
            m_ThemeMap.Add(Theme.Blue, vS2015BlueTheme1);
        }

        private void RegisterFormHotkeys(IHotkeyCallbacks HotkeyCallbacks)
        {
            HotkeyCallbacks.RegisterForm(this);
            HotkeyCallbacks.Register(Keys.F1, () => playButton_Click(null, null));
            HotkeyCallbacks.Register(Keys.F2, () => recordButton_Click(null, null));
        }

        private void OnFormActivated(object sender, EventArgs e)
        {
            this.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                this.Text = ProjectManager.ProjectName + " - " + Paths.AppName;
            }));
        }

        #region New Test Fixture Window Addition

        private void OnFixtureAdded(TestFixture fixture)
        {
            TestFixtureWindows = GetNotDestroyedWindows(TestFixtureWindows).ToList();
            var window = Container.Resolve<TestFixtureWindow>();

            ShowWindowPreferablyUponAnotherTestFixtureWindowAsTab(window);

            SetTestFixtureWindowTheme((TestFixtureWindow)window, m_VsVersion, m_ThemeMap[m_CurrentTheme]);

            window.DisplayTestFixture(fixture);
            window.OnSelectionChanged += ShowSelectedObjectInInspector;

            // Update font for newly added fixture and register form for future font changes
            RegisterAllFormsToTheFontManager();
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

        #endregion // New Test Fixture Window Addition

        #region Editor Settings

        private void OnSettingsRestored()
        {
            var settings = SettingsManager.GetSettings<EditorSettings>();

            this.BeginInvokeIfCreated(new MethodInvoker(delegate
            {
                FontManager.ForceUpdateFonts();

                // Update combo box items if they were modifed from properties window
                actionOnPlay.SelectedIndex = (int)settings.PlayingAction;
                actionOnRec.SelectedIndex = (int)settings.RecordingAction;

                // Update theme if it has changed
                if (m_CurrentTheme != settings.Theme || m_IsFirstTimeRestoringLayout)
                    SetWindowTheme(settings.Theme);
            }));
        }

        private void OnSettingsModified(BaseSettings setting)
        {
            if (setting is DesignSettings)
                this.BeginInvokeIfCreated(new MethodInvoker(FontManager.ForceUpdateFonts));

            if (setting is EditorSettings)
            {
                var editorSettings = SettingsManager.GetSettings<EditorSettings>();
                this.BeginInvokeIfCreated(new MethodInvoker(delegate
                {
                    // Update combo box items if they were modifed from properties window
                    actionOnPlay.SelectedIndex = (int)editorSettings.PlayingAction;
                    actionOnRec.SelectedIndex = (int)editorSettings.RecordingAction;

                    // Update theme if it has changed
                    if (m_CurrentTheme != editorSettings.Theme || m_IsFirstTimeRestoringLayout)
                        SetWindowTheme(editorSettings.Theme);
                }));
            }
        }

        private void actionOnRec_SelectedIndexChanged(object sender, EventArgs e)
        {
            var settings = SettingsManager.GetSettings<EditorSettings>();
            settings.RecordingAction = (WindowState)actionOnRec.SelectedIndex;
            m_PropertiesWindow.UpdateCurrentProperties();
        }

        private void actionOnPlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            var settings = SettingsManager.GetSettings<EditorSettings>();
            settings.PlayingAction = (WindowState)actionOnPlay.SelectedIndex;
            m_PropertiesWindow.UpdateCurrentProperties();
        }

        private void OnPlayingStateChanged(bool isPlaying)
        {
            this.BeginInvokeIfCreated(new MethodInvoker(delegate
            {
                var settings = SettingsManager.GetSettings<EditorSettings>();

                if (isPlaying && settings.PlayingAction == Robot.Settings.WindowState.Minimize)
                {
                    m_DefaultWindowState = this.WindowState;
                    this.WindowState = FormWindowState.Minimized;
                }

                if (!isPlaying && settings.PlayingAction == Robot.Settings.WindowState.Minimize)
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
            var settings = SettingsManager.GetSettings<EditorSettings>();

            if (isRecording && settings.RecordingAction == Robot.Settings.WindowState.Minimize)
            {
                m_DefaultWindowState = this.WindowState;
                this.WindowState = FormWindowState.Minimized;
            }

            if (!isRecording && settings.RecordingAction == Robot.Settings.WindowState.Minimize)
            {
                this.WindowState = m_DefaultWindowState;
                this.Activate();
                this.Focus();
            }

            UpdateToolstripButtonStates();
        }

        #endregion Editor Settings

        private void OnVisualizationStateChanged(bool isVisualizationOn)
        {
            UpdateToolstripButtonStates();
        }

        private void OnAssetSelected()
        {
            var asset = m_AssetsWindow.GetSelectedAsset();
            if (asset == null)
                return;

            if (asset.ImporterType != typeof(ImageImporter))
                return;

            asset.Load<object>(); // Force loading before check
            if (asset.LoadingFailed)
                return;

            m_ScreenPreviewWindow.Preview(asset);
            if (MouseRobot.IsVisualizationOn && asset.HoldsTypeOf(typeof(Bitmap)))
            {
                var settings = SettingsManager.GetSettings<DetectionSettings>();
                FeatureDetectionThread.StartNewImageSearch(m_AssetsWindow.GetSelectedAsset().Load<Bitmap>(), settings.ImageDetectionMode);
            }
        }

        private void ShowSelectedObjectInInspector(IBaseHierarchyManager BaseHierarchyManager, object obj)
        {
            m_InspectorWindow.ShowObject(obj, BaseHierarchyManager);
        }

        private Status m_LastStatus;
        private void OnStatusStripHandleCreated(object sender, EventArgs e) => OnStatusUpdated(m_LastStatus);

        private void OnStatusUpdated(Status status)
        {
            m_LastStatus = status;
            if (!statusStrip.Created || this.Disposing || this.IsDisposed ||
                statusStrip.IsDisposed || statusStrip.Disposing)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
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

        private void SetWindowTheme(Theme theme)
        {
            if (m_CurrentTheme == theme && !m_IsFirstTimeRestoringLayout)
                return;

            m_CurrentTheme = theme;
            var newThemeBase = m_ThemeMap[theme];

            if (!m_IsFirstTimeRestoringLayout)
            {
                DockLayout.Save(m_DockPanel);
                DockLayout.CloseAllContents(m_DockPanel);
            }

            m_DockPanel.Theme = newThemeBase;

            EnableVSDesignForToolstrips(m_VsVersion, newThemeBase);

            if (m_DockPanel.Theme.ColorPalette != null)
                statusStrip.BackColor = m_DockPanel.Theme.ColorPalette.MainWindowStatusBarDefault.Background;

            DockLayout.Restore(m_DockPanel);

            m_IsFirstTimeRestoringLayout = false;
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

        private void RegisterAllFormsToTheFontManager()
        {
            FontManager.Forms.Clear();
            FontManager.Forms.Add(this);
            FontManager.Forms.AddRange(m_Windows);
            FontManager.Forms.AddRange(TestFixtureWindows);

            this.BeginInvokeIfCreated(new MethodInvoker(FontManager.ForceUpdateFonts));
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
            var settings = SettingsManager.GetSettings<EditorSettings>();
            settings.Theme = Theme.Dark;

            if (m_CurrentTheme != settings.Theme)
                SetWindowTheme(settings.Theme);
        }

        private void blueThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = SettingsManager.GetSettings<EditorSettings>();
            settings.Theme = Theme.Blue;

            if (m_CurrentTheme != settings.Theme)
                SetWindowTheme(settings.Theme);
        }

        private void lightThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = SettingsManager.GetSettings<EditorSettings>();
            settings.Theme = Theme.Light;

            if (m_CurrentTheme != settings.Theme)
                SetWindowTheme(settings.Theme);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var didOpen = ProjectSelectionDialog.OpenNewProgramInstanceOfProjectWithDialog();
            if (didOpen)
                Close();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var didOpen = ProjectSelectionDialog.OpenNewProgramInstanceOfProjectWithDialog();
            if (didOpen)
                Close();
        }

        private void newTestFixtureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestFixtureManager.NewTestFixture();
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
            m_PropertiesWindow.ShowSettings(SettingsManager.GetSettings<DetectionSettings>());
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

            visualizationButton.ToolTipText = (MouseRobot.IsVisualizationOn) ?
                Properties.Resources.S_DisableImageDetection : Properties.Resources.S_EnableImageDetection;

            textDetectionButton.Image = (MouseRobot.IsTextDetectionOn) ?
                Properties.Resources.Text_e_ICO_32 : Properties.Resources.Text_d_ICO_32;

            textDetectionButton.ToolTipText = (MouseRobot.IsTextDetectionOn) ?
                Properties.Resources.S_DisableTextDetection : Properties.Resources.S_EnableTextDetection;

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
            TextDetectionThread.Stop();
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((Form)ScreenPaintForm).Close();

            DockLayout.Save(m_DockPanel);

            MouseRobot.IsRecording = false;
            MouseRobot.IsPlaying = false;
        }
    }
}
