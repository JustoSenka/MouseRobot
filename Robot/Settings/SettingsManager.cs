using System;

namespace Robot.Settings
{
    public class SettingsManager
    {
        public RecordingSettings RecordingSettings { get; private set; }




        static private SettingsManager m_Instance = new SettingsManager();
        static public SettingsManager Instance { get { return m_Instance; } }
        private SettingsManager()
        {
            RecordingSettings = new RecordingSettings();


        }
    }
}
