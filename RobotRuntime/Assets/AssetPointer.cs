using System;

namespace RobotRuntime
{
    [Serializable]
    public struct AssetPointer
    {
        public string Path { get; set; }
        public Int64 Hash { get; set; }

        public AssetPointer(string path, Int64 hash)
        {
            Path = path;
            Hash = hash;
        }
    }
}
