using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Robot
{
    public class Asset
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public Int64 Hash { get; private set; }

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

        public override string ToString()
        {
            return Name;
        }
    }
}
