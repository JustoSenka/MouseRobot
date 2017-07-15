using Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    public static class FileExtension
    {
        public const string Script = "mrb";
        public const string ScriptD = ".mrb";

        public const string Timeline = "mrt";
        public const string TimelineD = ".mrt";
    }

    public static class RegexExpression
    {
        public const string GetScriptNameFromPath = @"[/\\]{1}[\w\d]+\." + FileExtension.Script; 
        public const string GetTimelineNameFromPath = @"[/\\]{1}[\w\d]+\." + FileExtension.Timeline;
    }
}
