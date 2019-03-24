using RobotRuntime.Abstractions;
using System.ComponentModel;
using System.Linq;

namespace RobotEditor.PropertyUtils
{
    public class DetectorNameStringConverter : StringConverter
    {
        private static IFeatureDetectorFactory FeatureDetectorFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(FeatureDetectorFactory.DetectorNames.ToArray());
        }
    }

    public class DetectorNameStringConverterWithDefault : StringConverter
    {
        private static IFeatureDetectorFactory FeatureDetectorFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(FeatureDetectorFactory.DetectorNames.Append("Default").ToArray());
        }
    }
}
