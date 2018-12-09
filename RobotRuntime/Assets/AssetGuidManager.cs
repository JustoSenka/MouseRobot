using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RobotRuntime.Assets
{
    public class AssetGuidManager : IAssetGuidManager
    {
        public const string GuidPathMapFileName = "AssetGuidsTable.meta";
        public const string GuidHashMapFileName = "AssetHashTable.meta";
        
        private string GuidPathMapFilePath { get { return Path.Combine(RobotRuntime.Utils.Paths.MetadataFolder, GuidPathMapFileName); } }
        private string GuidHashMapFilePath { get { return Path.Combine(RobotRuntime.Utils.Paths.MetadataFolder, GuidHashMapFileName); } }

        private Dictionary<Guid, string> m_GuidPathMap = new Dictionary<Guid, string>();
        private Dictionary<Guid, Int64> m_GuidHashMap = new Dictionary<Guid, Int64>();

        private ObjectIO m_Serializer;
        private bool m_GuidPathMapDirty;
        private bool m_GuidHashMapDirty;

        public AssetGuidManager()
        {
            m_Serializer = new YamlDotNetIO();
        }

        public void AddNewGuid(Guid guid, string path, Int64 hash)
        {
            var isHashTheSame = GetHash(guid) == hash;
            var isPathTheSame = GetPath(guid) == path;

            m_GuidHashMap[guid] = hash;
            m_GuidPathMap[guid] = path;

            if (!isHashTheSame)
                m_GuidHashMapDirty = true;

            if (!isPathTheSame)
                m_GuidPathMapDirty = true;
        }

        public string GetPath(Guid guid)
        {
            string path;
            var success = m_GuidPathMap.TryGetValue(guid, out path);
            return success ? path : "";
        }

        public Int64 GetHash(Guid guid)
        {
            Int64 hash;
            var success = m_GuidHashMap.TryGetValue(guid, out hash);
            return success ? hash : default(Int64);
        }

        public Guid GetGuid(string path)
        {
            return m_GuidPathMap.FirstOrDefault(pair => pair.Value == path).Key;
        }

        public Guid GetGuid(Int64 hash)
        {
            return m_GuidHashMap.FirstOrDefault(pair => pair.Value == hash).Key;
        }

        public bool ContainsValue(string path)
        {
            return m_GuidPathMap.ContainsValue(path);
        }

        public bool ContainsValue(Int64 hash)
        {
            return m_GuidHashMap.ContainsValue(hash);
        }

        public IEnumerable<KeyValuePair<Guid, string>> Paths
        {
            get
            {
                foreach (var e in m_GuidPathMap)
                    yield return e;
            }
        }

        public IEnumerable<KeyValuePair<Guid, Int64>> Hashes
        {
            get
            {
                foreach (var e in m_GuidHashMap)
                    yield return e;
            }
        }

        public void LoadMetaFiles()
        {
            if (!File.Exists(GuidPathMapFilePath))
                m_GuidPathMap = new Dictionary<Guid, string>();

            if (!File.Exists(GuidHashMapFilePath))
                m_GuidHashMap = new Dictionary<Guid, Int64>();

            var newPathMap = m_Serializer.LoadObject<Dictionary<Guid, string>>(GuidPathMapFilePath);
            if (newPathMap != null)
                m_GuidPathMap = newPathMap;

            var newHashMap = m_Serializer.LoadObject<Dictionary<Guid, string>>(GuidPathMapFilePath);
            if (newHashMap != null)
                m_GuidPathMap = newHashMap;

            m_GuidHashMapDirty = false;
            m_GuidPathMapDirty = false;
        }

        public void Save()
        {
            if (m_GuidPathMapDirty)
                SavePathMap(m_Serializer);
            if (m_GuidHashMapDirty)
                SaveHashMap(m_Serializer);

            m_GuidHashMapDirty = false;
            m_GuidPathMapDirty = false;
        }

        private void SavePathMap(ObjectIO serializer)
        {
            serializer.SaveObject(GuidPathMapFilePath, m_GuidPathMap);
        }

        private void SaveHashMap(ObjectIO serializer)
        {
            serializer.SaveObject(GuidHashMapFilePath, m_GuidHashMap);
        }
    }
}
