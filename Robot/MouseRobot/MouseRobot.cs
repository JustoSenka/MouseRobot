using System;
using Robot.Utils.Win32;
using RobotRuntime;
using RobotRuntime.Graphics;
using System.ComponentModel;
using Robot.Settings;
using RobotRuntime.Settings;
using Robot.Recording;
using System.IO;

namespace Robot
{
    public class MouseRobot
    {
        static private MouseRobot m_Instance = new MouseRobot();
        static public MouseRobot Instance { get { return m_Instance; } }

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

        private MouseRobot()
        {
            SetupProjectPath();
            ScriptManager.Instance.NewScript();
            ScriptThread.Instance.Finished += OnScriptFinished;

            var a = RecordingManager.Instance; // Initializing, if nobody is referencing, sinlgeton is not created
        }

        public void ForceInit() { } // This is to make sure that mouse robot singleton is created
       

        public void StartScript()
        {
            if (ScriptManager.Instance.ActiveScript == null)
                return;

            ScriptThread.Instance.Start(ScriptManager.Instance.ActiveScript.ToLightScript());
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
                if (m_IsPlaying)
                    throw new InvalidOperationException("Cannot record while playing script.");

                if (value != m_IsRecording)
                {
                    InputCallbacks.Init();
                    m_IsRecording = value;
                    RecordingStateChanged?.Invoke(value);
                }
            }
        }

        public bool IsPlaying
        {
            get { return m_IsPlaying; }
            set
            {
                if (m_IsRecording)
                    throw new InvalidOperationException("Cannot play a script while is recording.");

                if (value != m_IsPlaying)
                {
                    m_IsPlaying = value;
                    PlayingStateChanged?.Invoke(m_IsPlaying);

                    if (m_IsPlaying)
                        StartScript();
                    else
                        ScriptThread.Instance.Stop();
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
                        RuntimeSettings.ApplySettings(SettingsManager.Instance.FeatureDetectionSettings);
                        ScreenStateThread.Instace.Start();
                        FeatureDetectionThread.Instace.Start();
                    }
                    else
                    {
                        ScreenStateThread.Instace.Stop();
                        FeatureDetectionThread.Instace.Stop();
                    }
                }
            }
        }

        private void SetupProjectPath()
        {
            ProjectPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\MProject";
            if (!Directory.Exists(ProjectPath))
                Directory.CreateDirectory(ProjectPath);

            Environment.CurrentDirectory = ProjectPath;
        }
    }
}
