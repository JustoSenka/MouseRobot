using Robot.Abstractions;
using Robot.Recordings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Robot.Tests
{
    public class TestFixture : BaseHierarchyManager, ISimilar, IHaveGuid
    {
        public const string DefaultTestFixtureName = "New Fixture";
        public const string DefaultTestName = "New Test";

        public const string k_Setup = "Setup";
        public const string k_TearDown = "TearDown";
        public const string k_OneTimeSetup = "OneTimeSetup";
        public const string k_OneTimeTeardown = "OneTimeTeardown";

        public Recording Setup { get { return GetRecordingWithName(k_Setup); } set { ReplaceRecordingWithName(k_Setup, value); } }
        public Recording TearDown { get { return GetRecordingWithName(k_TearDown); } set { ReplaceRecordingWithName(k_TearDown, value); } }
        public Recording OneTimeSetup { get { return GetRecordingWithName(k_OneTimeSetup); } set { ReplaceRecordingWithName(k_OneTimeSetup, value); } }
        public Recording OneTimeTeardown { get { return GetRecordingWithName(k_OneTimeTeardown); } set { ReplaceRecordingWithName(k_OneTimeTeardown, value); } }

        public IList<Recording> Tests { get { return GetAllTests(); } }
        public IList<Recording> Hooks { get { return GetAllHooks(); } }

        public Guid Guid { get; protected set; }

        public string Name { get; set; } = DefaultTestFixtureName;
        public string Path { get; set; } = "";

        private bool m_IsDirty;
        public bool IsDirty
        {
            set
            {
                if (m_IsDirty != value)
                    DirtyChanged?.Invoke(this);

                m_IsDirty = value;

                if (m_IsDirty == false)
                {
                    foreach (var s in LoadedRecordings)
                        s.IsDirty = false;
                }
            }
            get
            {
                return LoadedRecordings.Any(s => s.IsDirty) || m_IsDirty;
            }
        }

        public event Action<TestFixture> DirtyChanged;

        private ILogger Logger;
        public TestFixture(IAssetManager AssetManager, ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
            : base(CommandFactory, Profiler, Logger)
        {
            this.Logger = Logger;

            AddRecording(new Recording() { Name = k_Setup });
            AddRecording(new Recording() { Name = k_TearDown });
            AddRecording(new Recording() { Name = k_OneTimeSetup });
            AddRecording(new Recording() { Name = k_OneTimeTeardown });

            Guid = Guid.NewGuid();
        }

        public override Recording NewRecording(Recording clone = null)
        {
            var s = base.NewRecording(clone);
            s.Name = GetUniqueTestName(DefaultTestName);
            m_IsDirty = true;
            return s;
        }

        private string GetUniqueTestName(string name)
        {
            var newName = name;
            var i = 0;

            while (LoadedRecordings.Any(recording => recording.Name == newName))
                newName = name + ++i;

            return newName;
        }

        public override void RemoveRecording(Recording recording)
        {
            base.RemoveRecording(recording);
            m_IsDirty = true;
        }

        public override void RemoveRecording(int position)
        {
            base.RemoveRecording(position);
            m_IsDirty = true;
        }

        public override Recording AddRecording(Recording recording, bool removeRecordingWithSamePath = false)
        {
            m_IsDirty = true;
            return base.AddRecording(recording, removeRecordingWithSamePath);
        }

        private Recording GetRecordingWithName(string name)
        {
            return LoadedRecordings.FirstOrDefault(s => s.Name == name);
        }

        private void ReplaceRecordingWithName(string name, Recording value)
        {
            var s = GetRecordingWithName(name);
            if (s == null)
                Logger.Logi(LogType.Error, "Tried to replace recording value with name '" + name + "' but it was not found.");

            var addedRecordingIndex = LoadedRecordings.Count - 1;
            var instertIntoIndex = LoadedRecordings.IndexOf(s);
            RemoveRecording(s);
            AddRecording(s);
            MoveRecordingBefore(addedRecordingIndex, instertIntoIndex);
        }

        public bool Similar(object obj)
        {
            var f = obj as TestFixture;
            if (f == null)
                return false;

            if (f.Name != Name)
                return false;

            return LoadedRecordings.SequenceEqual(f.LoadedRecordings, new SimilarEqualityComparer());
        }

        /// <summary>
        /// Implemented explicitly so it has less visibility, since most systems should not regenerate guids by themself.
        /// As of this time, only recordings need to regenerate guids for commands (2018.08.15)
        /// </summary>
        void IHaveGuid.RegenerateGuid()
        {
            Guid = Guid.NewGuid();
        }

        void IHaveGuid.OverrideGuid(Guid newGuid)
        {
            Guid = newGuid;
        }

        public override int GetHashCode()
        {
            int hash = Name.GetHashCode();
            hash += LoadedRecordings.Select(recording => recording.GetHashCode()).Sum();
            return hash;
        }

        private IList<Recording> GetAllTests()
        {
            return LoadedRecordings.Where(s => s.Name != k_Setup && s.Name != k_TearDown && s.Name != k_OneTimeSetup && s.Name != k_OneTimeTeardown).ToList();
        }

        private IList<Recording> GetAllHooks()
        {
            return LoadedRecordings.Where(s => s.Name == k_Setup || s.Name == k_TearDown || s.Name == k_OneTimeSetup || s.Name == k_OneTimeTeardown).ToList();
        }

        // Inheritence

        public LightTestFixture ToLightTestFixture()
        {
            return new LightTestFixture(Guid)
            {
                Tests = Tests,
                Setup = Setup,
                OneTimeSetup = OneTimeSetup,
                TearDown = TearDown,
                OneTimeTeardown = OneTimeTeardown,
                Name = Name
            };
        }

        public TestFixture ApplyLightFixtureValues(LightTestFixture t)
        {
            RemoveAllRecordings();

            Name = t.Name;
            Guid = t.Guid == default(Guid) ? Guid : t.Guid;
            
            AddRecording(t.Setup);
            AddRecording(t.TearDown);
            AddRecording(t.OneTimeSetup);
            AddRecording(t.OneTimeTeardown);
            
            foreach (var test in t.Tests)
                AddRecording(test);

            m_IsDirty = false;

            return this;
        }

        public override string ToString()
        {
            return IsDirty ? Name + "*" : Name;
        }
    }
}
