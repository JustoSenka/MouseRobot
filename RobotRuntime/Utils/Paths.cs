using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RobotRuntime.Utils
{
    public static class Paths
    {
        public static string ScriptFolder { get { return "Scripts"; } }
        public static string ImageFolder { get { return "Images"; } }
        public static string PluginFolder { get { return "Plugins"; } }
        public static string MetadataFolder { get { return "Metadata"; } }

        public static string ScriptPath { get { return Path.Combine(Environment.CurrentDirectory, ScriptFolder); } }
        public static string ImagePath { get { return Path.Combine(Environment.CurrentDirectory, ImageFolder); } }
        public static string PluginPath { get { return Path.Combine(Environment.CurrentDirectory, ImageFolder); } }
        public static string MetadataPath { get { return Path.Combine(Environment.CurrentDirectory, MetadataFolder); } }

        public static string[] PathArray
        {
            get { return new[] { ScriptPath, ImagePath, PluginPath, MetadataPath }; }
        }

        public static string GetName(string path)
        {
            /*var name = Regex.Match(path, @"[/\\]{1}[^\\^/.]+\.\w{2,8}$").Value.
                TrimStart('/', '\\').
                TrimEnd(FileExtensions.Script.ToCharArray()).
                TrimEnd(FileExtensions.Timeline.ToCharArray()).
                TrimEnd(FileExtensions.Image.ToCharArray()).
                TrimEnd('.');*/

            return Path.GetFileNameWithoutExtension(path);
        }

        public static IEnumerable<string> GetAllFilePaths()
        {
            foreach (var path in PathArray)
            {
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
            if (folder == ScriptFolder)
                return FileExtensions.Script;
            else if (folder == ImageFolder)
                return FileExtensions.Image;
            else if (folder == PluginFolder)
                return FileExtensions.Plugin;
            else
                return "";
        }

        public static string GetFolderFromExtension(string path)
        {
            if (path.EndsWith(FileExtensions.Script))
                return ScriptFolder;
            else if (path.EndsWith(FileExtensions.Image))
                return ImageFolder;
            else if (path.EndsWith(FileExtensions.Plugin))
                return PluginFolder;
            else if (path.EndsWith(FileExtensions.Timeline))
                return "Timeline";
            else
                return "";
        }
    }
}
