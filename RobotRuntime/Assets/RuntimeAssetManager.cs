using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime.Assets
{
    /// <summary>
    /// This class is responsible for loading all assets when running tests.
    /// Assets are either preloaded or loaded from importers when in need.
    /// Do not load assets on initialize of editor, since it will take awhile.
    /// </summary>
    public class RuntimeAssetManager : IRuntimeAssetManager
    {
        private readonly Dictionary<Guid, object> m_GuidAssetMap = new Dictionary<Guid, object>();
        private readonly Dictionary<Guid, AssetImporter> m_GuidImporterMap = new Dictionary<Guid, AssetImporter>();

        private bool m_PreloadedAssetsUpToDate = false;

        private ILogger Logger;
        private IAssetGuidManager AssetGuidManager;
        public RuntimeAssetManager(IAssetGuidManager AssetGuidManager, ILogger Logger)
        {
            this.Logger = Logger;
            this.AssetGuidManager = AssetGuidManager;
        }

        public void CollectAllImporters()
        {
            m_GuidImporterMap.Clear();

            m_PreloadedAssetsUpToDate = false;

            var paths = Paths.GetAllFilePaths();

            foreach (var path in paths)
            {
                var importer = AssetImporter.FromPath(path);
                if (Logger.AssertIf(importer == null, "Cannot create importer on path: " + path)) continue;

                var guid = AssetGuidManager.GetGuid(path);
                if (Logger.AssertIf(guid.IsDefault(), "AssetGuidManager did not have guid for asset: " + path + ". Did you forget to load metadata?")) continue;

                m_GuidImporterMap.Add(guid, importer);
            }
        }

        public void PreloadAllAssets()
        {
            if (m_GuidImporterMap.Count == 0)
                CollectAllImporters();

            m_GuidAssetMap.Clear();

            Logger.AssertIf(m_GuidImporterMap.Count == 0, "No Assets found in project");

            foreach (var pair in m_GuidImporterMap)
            {
                var value = pair.Value.Value; // If it fails to load, importer itself logs error
                if (value == null) continue;

                m_GuidAssetMap.Add(pair.Key, value);
            }

            m_PreloadedAssetsUpToDate = true;
        }

        public AssetImporter GetImporterForAsset(Guid guid)
        {
            AssetImporter importer;
            var success = m_GuidImporterMap.TryGetValue(guid, out importer);

            Logger.AssertIf(!success, "Asset importer is not found with GUID: " + guid);
            return importer;
        }

        public object GetAsset(Guid guid)
        {
            object value;
            if (m_PreloadedAssetsUpToDate) // Get asset directly from dictionary
            {
                var success = m_GuidAssetMap.TryGetValue(guid, out value);
                Logger.AssertIf(!success, "Asset is not found with GUID: " + guid);
            }
            else // Load asset from importer
            {
                var importer = GetImporterForAsset(guid);
                if (importer == null)
                    return null;

                value = importer.Value;
            }

            return value;
        }

        public T GetAsset<T>(Guid guid) where T : class
        {
            var value = GetAsset(guid);
            var castedValue = value as T;

            Logger.AssertIf(castedValue == null, "Could not cast asset to correct type: " + typeof(T) + " with guid: " + guid);

            return castedValue;
        }

        public IEnumerable<AssetImporter> AssetImporters
        {
            get
            {
                return m_GuidImporterMap.Select(pair => pair.Value);
            }
        }

        public IEnumerable<object> Assets
        {
            get
            {
                if (m_PreloadedAssetsUpToDate)
                    return m_GuidAssetMap.Select(pair => pair.Value);
                else
                    return m_GuidImporterMap.Select(pair => pair.Value.Value);
            }
        }
    }
}
