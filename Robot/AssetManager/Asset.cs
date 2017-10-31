using RobotRuntime;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Robot
{
    public class Asset
    {
        public string Name { get; private set; }
        public Int64 Hash { get; private set; }

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
            Importer = AssetImporter.FromPath(path);
            Name = Commons.GetName(path);
            Hash = GetHash(path);
        }

        public Int64 GetHash(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
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
