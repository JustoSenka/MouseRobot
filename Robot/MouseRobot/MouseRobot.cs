using Robot.Abstractions;
using System;
using System.ComponentModel;
using RobotRuntime.Abstractions;
using RobotRuntime;
using RobotRuntime.Settings;
using RobotRuntime.Logging;
using RobotRuntime.Utils;

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
                PluginLoader.AsyncOperationOnUI = value;
            }
        }
        private AsyncOperation m_AsyncOperationOnUI;

        private IScriptManager ScriptManager;
        private ITestRunner TestRunner;
        private IRecordingManager RecordingManager;
        private IRuntimeSettings RuntimeSettings;
        private IScreenStateThread ScreenStateThread;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ISettingsManager SettingsManager;
        private IInputCallbacks InputCallbacks;
        private IPluginLoader PluginLoader;
        private IStatusManager StatusManager;
        public MouseRobot(IScriptManager ScriptManager, ITestRunner TestRunner, IRecordingManager RecordingManager, IRuntimeSettings RuntimeSettings,
            IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread, ISettingsManager SettingsManager,
            IInputCallbacks InputCallbacks, IPluginLoader PluginLoader, IStatusManager StatusManager)
        {
            this.ScriptManager = ScriptManager;
            this.TestRunner = TestRunner;
            this.RecordingManager = RecordingManager;
            this.RuntimeSettings = RuntimeSettings;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.SettingsManager = SettingsManager;
            this.InputCallbacks = InputCallbacks;
            this.PluginLoader = PluginLoader;
            this.StatusManager = StatusManager;
            
            ScriptManager.NewScript();
            TestRunner.TestRunEnd += OnScriptFinished;
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
                    {
                        Logger.Log(LogType.Error, "Cannot record while playing script.");
                        return;
                    }

                    InputCallbacks.Init();
                    m_IsRecording = value;
                    RecordingManager.IsRecording = value;
                    RecordingStateChanged?.Invoke(value);

                    if (m_IsRecording)
                        StatusManager.Add("IsRecording", 6, new Status("Waiting for input", "Recording", StandardColors.Purple));
                    else
                        StatusManager.Add("IsRecording", 10, new Status(null, "Recording Finished", StandardColors.Default));
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
                    {
                        Logger.Log(LogType.Error, "Cannot play a script while is recording.");
                        return;
                    }

                    InputCallbacks.Init();
                    m_IsPlaying = value;
                    PlayingStateChanged?.Invoke(m_IsPlaying);
                    RuntimeSettings.ApplySettings(SettingsManager.GetSettings<FeatureDetectionSettings>());

                    if (m_IsPlaying)
                    {
                        StatusManager.Add("IsPlaying", 6, new Status("Busy", "Running Tests", StandardColors.Green));
                    }
                    else
                    {
                        TestRunner.Stop();
                        StatusManager.Add("IsPlaying", 10, new Status(null, "Tests Complete", StandardColors.Default));
                    }
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
                        RuntimeSettings.ApplySettings(SettingsManager.GetSettings<FeatureDetectionSettings>());
                        if (!ScreenStateThread.IsAlive)
                            ScreenStateThread.Start();

                        if (!FeatureDetectionThread.IsAlive)
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
