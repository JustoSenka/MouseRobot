using RobotRuntime;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Robot
{
    public class Asset
    {
        public string Name { get; private set; }
        public Int64 Hash { get; private set; }
        public AssetGUID GUID { get; private set; }

        public string Path
        {
            get { return Importer.Path; }
            internal set
            {
                Importer.Path = value;
                Name = Commons.GetName(value);
            }
        }

        public AssetImporter Importer { get; private set; }
        public Asset(string path)
        {
            UpdatePath(path);
        }

        /// <summary>
        /// Will reload asset from disk and update Hash and GUID
        /// </summary>
        public void UpdateValueFromDisk() { UpdateInternal(Path, true); }

        /// <summary>
        /// Will update Path, GUID and name of asset but references will still point to old value
        /// </summary>
        public void UpdatePath(string path) { UpdateInternal(path, false); }

        /// <summary>
        /// Will update GUID and Hash (not from disk, only if asset value was modified by script). Will keep old references
        /// </summary>
        public void Update() { UpdateInternal(Path, false); }

        private void UpdateInternal(string path, bool readDisk)
        {
            Importer = Importer == null || readDisk ? EditorAssetImporter.FromPath(path) : Importer;
            Importer.Path = path;
            Name = Commons.GetName(path);
            Hash = GetHash(path);
            GUID = new AssetGUID(Path, Hash);
        }

        static public Int64 GetHash(string filePath)
        {
            var bytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : Encoding.Unicode.GetBytes(filePath);
            byte[] hashBytes = MD5.Create().ComputeHash(bytes);
            return BitConverter.ToInt64(hashBytes, 0);
        }

        public bool HoldsTypeOf(Type type)
        {
            return Importer.HoldsType() == type;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
