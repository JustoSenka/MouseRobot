﻿using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface IAssetGuidManager
    {
        bool MetadataLoaded { get; }

        IEnumerable<KeyValuePair<Guid, long>> Hashes { get; }
        IEnumerable<KeyValuePair<Guid, string>> Paths { get; }

        void AddNewGuid(Guid guid, string path, long hash);
        bool ContainsValue(string path);
        bool ContainsValue(long hash);
        Guid GetGuid(string path);
        Guid GetGuid(long hash);
        long GetHash(Guid guid);
        string GetPath(Guid guid);
        void LoadMetaFiles();
        void Save();
    }
}