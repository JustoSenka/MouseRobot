using System.ComponentModel;
using System.Linq;
using Robot;
using System.Drawing;

namespace RobotEditor.Settings
{
    public class AssetPointerImageStringConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(AssetManager.Instance.Assets.Where(a => a.HoldsTypeOf(typeof(Bitmap))).Select(a => a.Name).ToList());
        }
    }
}
