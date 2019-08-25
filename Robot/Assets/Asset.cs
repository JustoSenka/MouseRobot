using RobotRuntime;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Robot
{
    [DebuggerDisplay("Name = {Name}")]
    public class Asset : IComparable<Asset>
    {
        public string Name { get; private set; }
        public Int64 Hash { get; private set; }
        public Guid Guid { get; private set; }

        /// <summary>
        /// Gets or Sets path from/for importer
        /// </summary>
        public string Path
        {
            get { return Importer.Path; }
            internal set
            {
                Importer.Path = value;
                Name = Paths.GetName(value);
            }
        }

        private AssetImporter Importer { get; set; }

        public Type ImporterType => Importer.GetType();

        /// <summary>
        /// Will create asset on path.
        /// Will not read file to calculate hash.
        /// </summary>
        /// <param name="path"></param>
        public Asset(string path)
        {
            Guid = Guid.NewGuid();
            UpdatePath(path);
        }

        /// <summary>
        /// If calculateHash is true, will read file contents.
        /// Should not be used if no hash is needed because it is slow for many assets.
        /// If calculateHash is false, will do the same as ctor with path only.
        /// </summary>
        public Asset(string path, bool calculateHash)
        {
            Guid = Guid.NewGuid();
            UpdateInternal(path, calculateHash);
        }

        /// <summary>
        /// This should only be called from AssetManager when refreshing assets, creating new assets.
        /// Manually editing guid from other parts of the code might result in unexpected behaviours
        /// </summary>
        internal void SetGuid(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        /// Will reload asset from disk and update Hash
        /// </summary>
        internal void UpdateValueFromDisk() { UpdateInternal(Path, true); }

        /// <summary>
        /// Will update Path and name of asset but references will still point to old value
        /// </summary>
        internal void UpdatePath(string path) { UpdateInternal(path, false); }

        /// <summary>
        /// Will update Hash (not from disk, only if asset value was modified by recording). Will keep old references
        /// </summary>
        internal void Update() { UpdateInternal(Path, true); }

        private void UpdateInternal(string path, bool readDisk)
        {
            Importer = Importer == null || readDisk ? EditorAssetImporter.FromPath(path) : Importer;
            Importer.Path = path;
            Name = Paths.GetName(path);
            Hash = GetHash(path, readDisk);
        }

        public static Int64 GetHash(string filePath, bool readDisk)
        {
            var bytes = File.Exists(filePath) && readDisk ? File.ReadAllBytes(filePath) : Encoding.Unicode.GetBytes(filePath);
            byte[] hashBytes = MD5.Create().ComputeHash(bytes);
            return BitConverter.ToInt64(hashBytes, 0);
        }

        public bool HoldsTypeOf(Type type)
        {
            return Importer.HoldsType() == type;
        }

        public override string ToString() => System.IO.Path.GetFileName(Path);

        public int CompareTo(Asset other)
        {
            return System.IO.Path.GetFileName(Path).CompareTo(System.IO.Path.GetFileName(other.Path));
        }

        // Forwarding calls to AssetImporter

        public object Value
        {
            get
            {
                return Importer.Value;
            }
            set
            {
                Importer.Value = value;
            }
        }

        public Type HoldsType() => Importer.HoldsType();
        public bool LoadingFailed => Importer.LoadingFailed;

        // Load, Reload and Save asset methods all have fix guid functionality.
        // Idea is asset guid and object guid (if has one) should all be the same.
        // AssetManager makes sure assets have all unique guids,
        // so Recordings Or TestFixtures don't need to worry about it

        public T Load<T>()
        {
            var value = Importer.Load<T>();
            if (FixObjectGuidIfNeeded(value))
                SaveAsset();

            return value;
        }
        public T ReloadAsset<T>()
        {
            var value = Importer.ReloadAsset<T>();
            if (FixObjectGuidIfNeeded(value))
                SaveAsset();

            return value;
        }

        public void SaveAsset()
        {
            FixObjectGuidIfNeeded(Value);
            Importer.SaveAsset();
        }

        /// <summary>
        /// If object has IHaveGuid and the guid is not the same as asset guid, 
        /// overrides it with asset guid so they are the same and unique
        /// </summary>
        /// <returns>True if Guid was overriden</returns>
        private bool FixObjectGuidIfNeeded(object importedObject)
        {
            if (importedObject is IHaveGuid obj && obj.Guid != Guid)
            {
                obj.OverrideGuid(Guid);
                return true;
            }
            return false;
        }
    }
}
