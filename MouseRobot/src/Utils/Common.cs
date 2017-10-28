using Robot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Robot
{
    public static class Commons
    {
        public static string GetName(string path)
        {
            var name = Regex.Match(path, @"[/\\]{1}[\w\d ]+\.").Value.
                TrimStart('/', '\\').
                TrimEnd(FileExtensions.Script.ToCharArray()).
                TrimEnd(FileExtensions.Timeline.ToCharArray()).
                TrimEnd(FileExtensions.Image.ToCharArray()).
                TrimEnd('.');

            return name;
        }
    }

    public static class FileExtensions
    {
        public const string Script = "mrb";
        public const string ScriptD = ".mrb";

        // TODO: Obiously need to improve on this side
        public const string Image = "png";
        public const string ImageD = ".png";

        public const string Timeline = "mrt";
        public const string TimelineD = ".mrt";
    }

    public static class RegexExpression
    {
        public const string GetScriptNameFromPath = @"[/\\]{1}[\w\d ]+\." + FileExtensions.Script;
        public const string GetImageNameFromPath = @"[/\\]{1}[\w\d ]+\." + FileExtensions.Image;
        public const string GetTimelineNameFromPath = @"[/\\]{1}[\w\d ]+\." + FileExtensions.Timeline;
    }

    public static class Fonts
    {
        public static Font Default = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Regular);
        public static Font DirtyScript = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Italic);
        public static Font ActiveScript = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Bold);

        public static Font ActiveAndDirtyScript = DirtyScript.AddFont(ActiveScript);

        public static Font AddFont(this Font main, Font newFont)
        {
            return new Font(
                (newFont.FontFamily == Default.FontFamily) ? main.FontFamily : newFont.FontFamily,
                (newFont.Size == Default.Size) ? main.Size : newFont.Size,
                main.Style | newFont.Style);
        }

        public static Font RemoveFont(this Font main, Font remove)
        {
            return new Font(
                (remove.FontFamily == main.FontFamily) ? Default.FontFamily : main.FontFamily,
                (remove.Size == main.Size) ? Default.Size : main.Size,
                main.Style & ~remove.Style);
        }
    }
}
