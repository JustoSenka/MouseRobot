﻿using System.Drawing;
using System.Text.RegularExpressions;

namespace RobotRuntime
{
    public static class FileExtensions
    {
        public const string Recording = "mrb";
        public const string RecordingD = ".mrb";

        public const string Image = "png";
        public const string ImageD = ".png";

        public const string Script = "cs";
        public const string ScriptD = ".cs";

        public const string Test = "mrt";
        public const string TestD = ".mrt";

        public const string Dll = "dll";
        public const string DllD = ".dll";

        public const string Exe = "exe";
        public const string ExeD = ".exe";
    }

    public static class RegexExpression
    {
        public const string GetNameWithDot = @"[/\\]{1}[\w\d ]+\.";
        public const string GetRecordingNameFromPath = GetNameWithDot + FileExtensions.Recording;
        public const string GetImageNameFromPath = GetNameWithDot + FileExtensions.Image;
        public const string GetScriptNameFromPath = GetNameWithDot + FileExtensions.Script;
        public const string GetTestNameFromPath = GetNameWithDot + FileExtensions.Test;
        public const string Guid = @"[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?";
    }

    public static class Fonts
    {
        public static Font Default = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Regular);
        public static Font Big = new Font(FontFamily.GenericSansSerif, 20F, FontStyle.Bold);
        public static Font Normal = new Font(FontFamily.GenericSansSerif, 12F, FontStyle.Bold);

        public static Font AppendStyle(this Font main, FontStyle style) => new Font(main.FontFamily, main.SizeInPoints, style);
    }
}
