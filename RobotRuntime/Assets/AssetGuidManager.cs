using RobotRuntime.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotRuntime.Assets
{
    public class AssetGuidManager
    {
        static private AssetGuidManager m_Instance = new AssetGuidManager();
        static public AssetGuidManager Instance { get { return m_Instance; } }
        private AssetGuidManager() { }

        public const string MetadataFolder = "Metadata";
        public const string AssetGuidsTableFilename = "AssetGuidsTable.meta";

        public string MetadataPath {  get { return Path.Combine(Environment.CurrentDirectory, MetadataFolder); } }
        private string AssetGuidsTablePath {  get { return Path.Combine(MetadataFolder, AssetGuidsTableFilename); } }

        private Dictionary<Guid, string> m_AssetGuidsMap = new Dictionary<Guid, string>();

        public void AddPathToGuid(Guid guid, string path)
        {
            m_AssetGuidsMap[guid] = path;
            Save(); // Super slow
        }

        public void RemoveGuid(Guid guid)
        {
            m_AssetGuidsMap.Remove(guid);
            Save(); // Super slow
        }

        public string GetPath(Guid guid)
        {
            string path;
            var success = m_AssetGuidsMap.TryGetValue(guid, out path);
            return success ? path : "";
        }

        public Guid GetGuid(string path)
        {
            return m_AssetGuidsMap.FirstOrDefault(pair => pair.Value == path).Key;
        }

        public bool ContainsValue(string path)
        {
            return m_AssetGuidsMap.ContainsValue(path);
        }

        public IEnumerable<KeyValuePair<Guid, string>> GetEnumarable()
        {
            foreach (var e in m_AssetGuidsMap)
                yield return e;
        }

        public void LoadMetaFiles()
        {
            if (!File.Exists(AssetGuidsTablePath))
            {
                m_AssetGuidsMap = new Dictionary<Guid, string>();
                return;
            }

            var serializer = new YamlObjectIO();
            var newMap = serializer.LoadObject<Dictionary<Guid, string>>(AssetGuidsTablePath);

            //if (newMap != null)
                m_AssetGuidsMap = newMap;
        }

        private void Save()
        {
            var serializer = new YamlObjectIO();
            serializer.SaveObject(AssetGuidsTablePath, m_AssetGuidsMap);
        }
    }
}
