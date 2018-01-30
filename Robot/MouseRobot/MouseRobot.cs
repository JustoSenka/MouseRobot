using Robot.Abstractions;
using System;
using Robot.Utils.Win32;
using System.ComponentModel;
using System.IO;
using RobotRuntime.Abstractions;

namespace Robot
{
    public class MouseRobot : IMouseRobot
    {
        public event Action<bool> RecordingStateChanged;
        public event Action<bool> PlayingStateChanged;
        public event Action<bool> VisualizationStateChanged;

        private bool m_IsRecording;
        private bool m_IsPlaying;
        private bool m_IsVisualizationOn;

        /// <summary>
        /// Post messages to async operation in order to run them on UI thread
        /// </summary>
        public AsyncOperation AsyncOperationOnUI
        {
            get { return m_AsyncOperationOnUI; }
            set
            {
                m_AsyncOperationOnUI = value;
                InputCallbacks.AsyncOperationOnUI = value;
            }
        }
        private AsyncOperation m_AsyncOperationOnUI;

        public string ProjectPath { get; private set; }

        private IScriptManager ScriptManager;
        private ITestRunner TestRunner;
        private IRecordingManager RecordingManager;
        private IRuntimeSettings RuntimeSettings;
        private IScreenStateThread ScreenStateThread;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ISettingsManager SettingsManager;
        private IInputCallbacks InputCallbacks;
        private IProjectManager ProjectManager;
        public MouseRobot(IScriptManager ScriptManager, ITestRunner TestRunner, IRecordingManager RecordingManager, IRuntimeSettings RuntimeSettings,
            IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread, ISettingsManager SettingsManager,
            IInputCallbacks InputCallbacks, IProjectManager ProjectManager)
        {
            this.ScriptManager = ScriptManager;
            this.TestRunner = TestRunner;
            this.RecordingManager = RecordingManager;
            this.RuntimeSettings = RuntimeSettings;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.SettingsManager = SettingsManager;
            this.InputCallbacks = InputCallbacks;
            this.ProjectManager = ProjectManager;

            ProjectManager.InitProject(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\MProject");

            ScriptManager.NewScript();
            TestRunner.Finished += OnScriptFinished;
        }

        public void StartScript()
        {
            if (ScriptManager.ActiveScript == null)
                return;

            TestRunner.Start(ScriptManager.ActiveScript.ToLightScript());
        }

        private void OnScriptFinished()
        {
            IsPlaying = false;
        }

        public bool IsRecording
        {
            get { return m_IsRecording; }
            set
            {
                if (value != m_IsRecording)
                {
                    if (m_IsPlaying)
                        throw new InvalidOperationException("Cannot record while playing script.");

                    InputCallbacks.Init();
                    m_IsRecording = value;
                    RecordingManager.IsRecording = value;
                    RecordingStateChanged?.Invoke(value);
                }
            }
        }

        public bool IsPlaying
        {
            get { return m_IsPlaying; }
            set
            {
                if (value != m_IsPlaying)
                {
                    if (m_IsRecording)
                        throw new InvalidOperationException("Cannot play a script while is recording.");

                    m_IsPlaying = value;
                    PlayingStateChanged?.Invoke(m_IsPlaying);
                    RuntimeSettings.ApplySettings(SettingsManager.FeatureDetectionSettings);

                    if (m_IsPlaying)
                        StartScript();
                    else
                        TestRunner.Stop();
                }
            }
        }

        public bool IsVisualizationOn
        {
            get { return m_IsVisualizationOn; }
            set
            {
                if (value != m_IsVisualizationOn)
                {
                    m_IsVisualizationOn = value;
                    VisualizationStateChanged?.Invoke(m_IsVisualizationOn);

                    if (m_IsVisualizationOn)
                    {
                        RuntimeSettings.ApplySettings(SettingsManager.FeatureDetectionSettings);
                        ScreenStateThread.Start();
                        FeatureDetectionThread.Start();
                    }
                    else
                    {
                        ScreenStateThread.Stop();
                        FeatureDetectionThread.Stop();
                    }
                }
            }
        }
    }
}
