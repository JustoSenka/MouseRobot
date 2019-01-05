using RobotEditor.Abstractions;
using RobotRuntime;
using System.Collections.Generic;
using System.Linq;

namespace RobotEditor.Resources.ScriptTemplates
{
    public class ScriptTemplates : IScriptTemplates
    {
        private readonly Dictionary<string, string> TemplateMap = new Dictionary<string, string>()
        {
            { "Command", "CustomCommand" },
            { "Command Properties", "CustomCommandDesigner" },
            { "Command Runner", "CustomCommandRunner" },
            { "Image Detector", "CustomFeatureDetector" },
            { "Screen Painter", "CustomScreenPainter" },
            { "Custom Settings", "CustomSettings" },
        };

        public string[] TemplateNames => TemplateMap.Keys.ToArray();

        public string GetTemplateFileName(string name)
        {
            TemplateMap.TryGetValue(name, out string fileName);

            if (fileName.IsEmpty())
                Logger.Log(LogType.Error, "Script Template does not seem to contain this resource: " + name);

            return fileName;
        }

        public string GetTemplate(string name)
        {
            var fileName = GetTemplateFileName(name);

            string value = "";
            if (!fileName.IsEmpty())
            {
                value = (string)Properties.Resources.ResourceManager.GetObject(fileName);

                if (value.IsEmpty())
                    Logger.Log(LogType.Error, "Resource Manager did not contain this resource: " + fileName + " from template name: " + name);
            }

            return value;
        }
    }
}
