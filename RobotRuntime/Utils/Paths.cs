using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RobotRuntime.Utils
{
    public static class Paths
    {
        public static string RecordingFolder { get { return "Recordings"; } }
        public static string ImageFolder { get { return "Images"; } }
        public static string ScriptFolder { get { return "Scripts"; } }
        public static string MetadataFolder { get { return "Metadata"; } }
        public static string TestsFolder { get { return "Tests"; } }
        public static string PluginFolder { get { return "Plugin"; } }

        public static string RecordingPath { get { return Path.Combine(Environment.CurrentDirectory, RecordingFolder); } }
        public static string ImagePath { get { return Path.Combine(Environment.CurrentDirectory, ImageFolder); } }
        public static string ScriptPath { get { return Path.Combine(Environment.CurrentDirectory, ScriptFolder); } }
        public static string MetadataPath { get { return Path.Combine(Environment.CurrentDirectory, MetadataFolder); } }
        public static string TestsPath { get { return Path.Combine(Environment.CurrentDirectory, TestsFolder); } }
        public static string PluginPath { get { return Path.Combine(Environment.CurrentDirectory, PluginFolder); } }

        public static string AppName { get { return "MouseRobot"; } }
        public static string RoamingAppdataPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Applicat‌​ionData), AppName); } }
        public static string LocalAppdataPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName); } }

        // public static string ApplicationInstallPath { get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); } } // Does not work from tests
        public static string ApplicationInstallPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        public static string[] ProjectPathArray
        {
            get { return new[] { RecordingPath, ImagePath, ScriptPath, MetadataPath, TestsFolder, PluginFolder }; }
        }

        /// <summary>
        /// Will add 0, 1, 2 until it finds unique path
        /// </summary>
        public static string GetUniquePath(string path)
        {
            var ext = Path.GetExtension(path);
            var pathNoExt = Path.Combine (Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));

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
            /*var name = Regex.Match(path, @"[/\\]{1}[^\\^/.]+\.\w{2,8}$").Value.
                TrimStart('/', '\\').
                TrimEnd(FileExtensions.Recording.ToCharArray()).
                TrimEnd(FileExtensions.Timeline.ToCharArray()).
                TrimEnd(FileExtensions.Image.ToCharArray()).
                TrimEnd('.');*/

            return Path.GetFileNameWithoutExtension(path);
        }

        public static IEnumerable<string> GetAllFilePaths(bool ignoreMetada = true)
        {
            foreach (var path in ProjectPathArray)
            {
                if (ignoreMetada && path == MetadataPath)
                    continue;

                foreach (string filePath in Directory.GetFiles(path))
                {
                    var folder = GetFolderFromExtension(filePath);
                    var name = Path.GetFileName(filePath);

                    if (folder != "" && name != null)
                        yield return folder + "\\" + name;
                }
            }
        }

        public static string GetNameWithExtension(string path)
        {
            var extension = Regex.Match(path, @"\.\w{2,8}$").Value;
            return GetName(path) + extension;
        }

        public static string GetFolder(string path)
        {
            return GetProjectRelativePath(path).Split('\\', '/')[0];
        }

        public static string GetProjectRelativePath(string path)
        {
            return Regex.Match(path, @"[^\\^/.]+[/\\]{1}[^\\^/.]+\.\w{2,8}$").Value;
        }

        public static bool AreRelativePathsEqual(string s, string d)
        {
            s = s.ToLower();
            d = d.ToLower();
            return s == d;
        }

        public static string GetExtensionFromFolder(string folder)
        {
            if (folder == RecordingFolder)
                return FileExtensions.Recording;
            else if (folder == ImageFolder)
                return FileExtensions.Image;
            else if (folder == ScriptFolder)
                return FileExtensions.Script;
            else if (folder == TestsFolder)
                return FileExtensions.Test;
            else if (folder == PluginFolder)
                return FileExtensions.Dll;
            else
                return "";
        }

        public static string GetFolderFromExtension(string path)
        {
            if (path.EndsWith(FileExtensions.Recording))
                return RecordingFolder;
            else if (path.EndsWith(FileExtensions.Image))
                return ImageFolder;
            else if (path.EndsWith(FileExtensions.Script))
                return ScriptFolder;
            else if (path.EndsWith(FileExtensions.Test))
                return TestsFolder;
            else if (path.EndsWith(FileExtensions.Dll) || path.EndsWith(FileExtensions.Exe))
                return PluginFolder;
            else
                return "";
        }
    }
}
