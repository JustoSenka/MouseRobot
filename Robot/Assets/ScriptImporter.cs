using RobotRuntime;
using RobotRuntime.Assets;
using RobotRuntime.IO;
using RobotRuntime.Scripts;
using System;

namespace Robot.Assets
{
    public class ScriptImporter : LightScriptImporter
    {
        public ScriptImporter(string path) : base(path) { }

        protected override object LoadAsset()
        {
            return new Script((LightScript)base.LoadAsset());
        }

        public override void SaveAsset()
        {
            new YamlScriptIO().SaveObject(Path, ((Script)Value).ToLightScript());
        }

        public override Type HoldsType()
        {
            return typeof(Script);
        }
    }
}
