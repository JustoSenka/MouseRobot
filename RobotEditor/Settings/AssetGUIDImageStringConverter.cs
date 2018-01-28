using System.ComponentModel;
using System.Linq;
using Robot;
using System.Drawing;
using Unity;
using Robot.Abstractions;

namespace RobotEditor.Settings
{
    public class AssetGUIDImageStringConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var AssetManager = RobotRuntime.Unity.Container.Resolve<IAssetManager>();
            return new StandardValuesCollection(AssetManager.Assets.Where(a => a.HoldsTypeOf(typeof(Bitmap))).Select(a => a.Name).ToList());
        }
    }
}
