using Robot.Abstractions;
using Robot.Recordings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.Linq;

namespace Robot
{
    public class HierarchyManager : BaseHierarchyManager, IHierarchyManager
    {
        public static string k_DefaultRecordingName = "New Recording";

        private Recording m_ActiveRecording;
        public Recording ActiveRecording
        {
            set
            {
                if (m_ActiveRecording != value)
                    ActiveRecordingChanged?.Invoke(m_ActiveRecording, value);

                m_ActiveRecording = value;
            }
            get { return m_ActiveRecording; }
        }

        public event Action<Recording, Recording> ActiveRecordingChanged;
        public event Action<Recording> RecordingSaved;

        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public HierarchyManager(IAssetManager AssetManager, ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
            : base(CommandFactory, Profiler, Logger)
        {
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;
        }

        public Recording LoadRecording(string path)
        {
            var asset = AssetManager.GetAsset(path);
            if (asset == null)
            {
                Logger.Log(LogType.Error, "Cannot load recording. No such asset at path: " + path);
                return null;
            }

            Profiler.Start("RecordingManager_LoadRecording");

            // if hierarchy contains empty untitled recording, remove it
            if (m_LoadedRecordings.Count == 1 && m_LoadedRecordings[0].Name == Recording.DefaultRecordingName && m_LoadedRecordings[0].Commands.Count() == 0)
                RemoveRecording(0);

            Recording newRecording = asset.Importer.ReloadAsset<Recording>();
            if (newRecording != null)
            {
                newRecording.Path = asset.Path;
                AddRecording(newRecording, true);
            }
            else
                Logger.Log(LogType.Error, "Failed to load recording: " + asset.Path);

            Profiler.Stop("RecordingManager_LoadRecording");
            return newRecording;
        }

        public void SaveRecording(Recording recording, string path)
        {
            Profiler.Start("RecordingManager_SafeRecording");

            AssetManager.CreateAsset(recording, path);
            recording.Path = Paths.GetProjectRelativePath(path);

            RecordingSaved?.Invoke(recording);

            Profiler.Stop("RecordingManager_SafeRecording");
        }

        private void MakeSureActiveRecordingExist()
        {
            if (!m_LoadedRecordings.Contains(ActiveRecording) || m_LoadedRecordings.Count == 0)
                ActiveRecording = null;

            if (m_LoadedRecordings.Count == 1)
                ActiveRecording = m_LoadedRecordings[0];

            if (ActiveRecording == null && m_LoadedRecordings.Count > 0)
                ActiveRecording = m_LoadedRecordings[0];
        }

        public override Recording NewRecording(Recording clone = null)
        {
            var s = base.NewRecording(clone);
            s.Name = k_DefaultRecordingName;
            MakeSureActiveRecordingExist();
            return s;
        }

        public override void RemoveRecording(Recording recording)
        {
            base.RemoveRecording(recording);
            MakeSureActiveRecordingExist();
        }

        public override void RemoveRecording(int position)
        {
            base.RemoveRecording(position);
            MakeSureActiveRecordingExist();
        }

        public override Recording AddRecording(Recording recording, bool removeRecordingWithSamePath = false)
        {
            var s = base.AddRecording(recording, removeRecordingWithSamePath);
            MakeSureActiveRecordingExist();
            return s;
        }
    }
}
