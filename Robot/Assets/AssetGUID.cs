using System;

namespace Robot.Assets
{
    public struct AssetGUID
    {
        public Int64 Hash { get; set; }
        public string Path { get; set; }

        public AssetGUID(Int64 hash, string path)
        {
            Hash = hash;
            Path = path;
        }
    }
}
