using System;

namespace RobotRuntime
{
    [Serializable]
    public struct AssetGUID
    {
        public string Path { get; set; }
        public Int64 Hash { get; set; }

        public AssetGUID(string path, Int64 hash)
        {
            Path = path;
            Hash = hash;
        }
    }
}
