using System;
using System.IO;
using System.Security.Cryptography;

namespace Robot
{
    public class Asset
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public Int64 Hash { get; private set; }

        private AssetImporter m_Importer;
        public AssetImporter Importer
        {
            get
            {
                if (m_Importer == null)
                    m_Importer = AssetImporter.FromPath(Path);

                return m_Importer;
            }
        }

        public Asset(string path)
        {
            Path = path;
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
