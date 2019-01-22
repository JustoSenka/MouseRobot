using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RobotRuntime.Utils
{
    public static class Paths
    {
        public static string AssetsFolder { get { return "Assets"; } }
        public static string MetadataFolder { get { return "Metadata"; } }

        public static string AssetsPath { get { return Path.Combine(Environment.CurrentDirectory, AssetsFolder); } }
        public static string MetadataPath { get { return Path.Combine(Environment.CurrentDirectory, MetadataFolder); } }

        public static string AppName { get { return "MouseRobot"; } }
        public static string RoamingAppdataPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Applicat‌​ionData), AppName); } }
        public static string LocalAppdataPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName); } }

        // public static string ApplicationInstallPath { get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); } } // Does not work from tests
        public static string ApplicationInstallPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        public static string[] ProjectPathArray
        {
            get { return new[] { AssetsPath, MetadataPath }; }
        }

        /// <summary>
        /// Will add 0, 1, 2 until it finds unique path
        /// </summary>
        public static string GetUniquePath(string path)
        {
            var ext = Path.GetExtension(path);
            var pathNoExt = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));

            if (!File.Exists(path))
                return path;

            string finalPath = "";
            var num = 0;
            do
            {
                finalPath = $"{pathNoExt}_{num++}{ext}";
            }
            while (File.Exists(finalPath));

            return finalPath;
        }

        public static string GetName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static IEnumerable<string> GetAllAssetPaths()
        {
            return Directory.GetFileSystemEntries(AssetsPath, "*", SearchOption.AllDirectories).Select(GetRelativePath);
        }

        public static string GetRelativePath(string fullPath)
        {
            var fullAbsolutePath = (File.Exists(fullPath)) ? new FileInfo(fullPath).FullName : new DirectoryInfo(fullPath).FullName;
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory).FullName;

            if (fullAbsolutePath.StartsWith(currentDirectory))
            {
                // The +1 is to avoid the directory separator
                return fullAbsolutePath.Substring(currentDirectory.Length + 1).Trim('\\', '/', ' ');
            }
            else
            {
                Logger.Log(LogType.Error, "Unable to make relative path from: " + fullAbsolutePath +
                    " where current environment path is: " + currentDirectory);
            }

            return ""; // Regex.Match(fullPath, @"[^\\^/.]+[/\\]{1}[^\\^/.]+\.\w{2,8}$").Value;
        }

        public static string[] GetPathDirectoryElements(string path)
        {
            return path.Split('\n').Select(NormalizePath).ToArray();
        }

        public static string NormalizePath(string path)
        {
            return path.Trim('\n', '\r', ' ', '\\', '/');
        }

        public static bool AreRelativePathsEqual(string s, string d)
        {
            s = s.ToLower();
            d = d.ToLower();
            return s == d;
        }
    }
}
