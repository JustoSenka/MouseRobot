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

            if (!File.Exists(path) && !Directory.Exists(path))
                return path;

            string finalPath = "";
            var num = 0;
            do
            {
                finalPath = $"{pathNoExt}_{num++}{ext}";
            }
            while (File.Exists(finalPath) || Directory.Exists(finalPath));

            return finalPath;
        }

        public static string GetName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static IEnumerable<string> GetAllAssetPaths(bool includeAssetPath = false)
        {
            var allAntries = Directory.GetFileSystemEntries(AssetsPath, "*", SearchOption.AllDirectories).Select(GetRelativePath);
            if (includeAssetPath)
                allAntries = allAntries.Append(AssetsFolder);

            return allAntries;
        }

        public static string GetRelativePath(string fullPath)
        {
            if (fullPath.IsEmpty())
                return fullPath;

            var fullAbsolutePath = (File.Exists(fullPath)) ? new FileInfo(fullPath).FullName : new DirectoryInfo(fullPath).FullName;
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory).FullName;

            if (fullAbsolutePath.StartsWith(currentDirectory))
            {
                // The +1 is to avoid the directory separator
                return fullAbsolutePath.Substring(currentDirectory.Length + 1).Trim('\\', '/', ' ');
            }
            else
            {
                Logger.Log(LogType.Error, "Unable to make relative pa.th from: " + fullAbsolutePath +
                    " where current environment path is: " + currentDirectory);
            }

            return ""; // Regex.Match(fullPath, @"[^\\^/.]+[/\\]{1}[^\\^/.]+\.\w{2,8}$").Value;
        }

        public static bool IsDirectory(string path)
        {
            if (path == null)
                return false;

            path = path.Trim();

            if (Directory.Exists(path))
                return true;

            if (File.Exists(path))
                return false;

            // neither file nor directory exists. guess intention

            // if has trailing slash then it's a directory
            if (new[] { "\\", "/" }.Any(x => path.EndsWith(x)))
                return true; // ends with slash

            // if has extension then its a file; directory otherwise
            return string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }

        /// <summary>
        /// Copies directory with all its contents to new path
        /// </summary>
        public static void CopyDirectories(string SourcePath, string DestinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
        }

        public static string GetPathParent(string path)
        {
            path = path.NormalizePath();
            var els = GetPathDirectoryElementsWtihFileName(path);
            return Path.Combine(els.Take(els.Length - 1).ToArray());
        }

        /// <summary>
        /// From path: "Assets/scripts/sc.mrb"
        /// Will return: new [] { "Assets", "scripts", "sc.mrb" }
        /// </summary>
        public static string[] GetPathDirectoryElementsWtihFileName(string path)
        {
            return path.Split('\\', '/').Select(NormalizePath).ToArray();
        }

        /// <summary>
        /// From path: "Assets/scripts/sc.mrb"
        /// Will return: new [] { "Assets", "scripts" }
        /// </summary>
        public static string[] GetPathDirectoryElements(string path)
        {
            var array = path.Split('\\', '/');
            var lastElement = array.Length - 1;

            return (Regex.IsMatch(array[lastElement], @"\w+\.\w{2,8}$")) ? // If last element is file with extension
                array.Take(lastElement).Select(NormalizePath).ToArray() : 
                array.Select(NormalizePath).ToArray();
        }

        /// <summary>
        /// From path elements: { "Assets", "scripts", "sc.mrb" }
        /// Will return: new [] { "Assets", "Assets/scripts", "Assets/scripts/sc.mrb" }
        /// </summary>
        public static string[] JoinDirectoryElementsIntoPaths(IEnumerable<string> elements)
        {
            var list = new List<string>();

            var count = elements.Count();
            for (int i = 0; i < count; i++)
                list.Add(string.Join(Path.DirectorySeparatorChar.ToString(), elements.Take(i + 1).ToArray()));

            return list.ToArray();
        }

        public static string NormalizePath(string path)
        {
            return path.Trim('\n', '\r', ' ', '\\', '/').
                Replace('/', Path.DirectorySeparatorChar).
                Replace('\\', Path.DirectorySeparatorChar);
        }

        public static bool AreRelativePathsEqual(string s, string d)
        {
            s = s.ToLower();
            d = d.ToLower();
            return s == d;
        }
    }
}
