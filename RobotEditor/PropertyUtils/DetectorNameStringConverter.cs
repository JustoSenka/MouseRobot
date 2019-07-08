using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Graphics;
using System.ComponentModel;
using System.Linq;

namespace RobotEditor.PropertyUtils
{
    public class DetectorNameStringConverter : StringConverter
    {
        [RequestStaticDependency(typeof(IFactoryWithCache<FeatureDetector>))]
        private static IFactoryWithCache<FeatureDetector> FeatureDetectorFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(FeatureDetectorFactory.DetectorNames.ToArray());
        }
    }

    public class DetectorNameStringConverterWithDefault : StringConverter
    {
        [RequestStaticDependency(typeof(IFactoryWithCache<FeatureDetector>))]
        private static IFactoryWithCache<FeatureDetector> FeatureDetectorFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(FeatureDetectorFactory.DetectorNames.Append("Default").ToArray());
        }
    }

    public class TextDetectorNameStringConverter : StringConverter
    {
        [RequestStaticDependency(typeof(IFactoryWithCache<TextDetector>))]
        private static IFactoryWithCache<TextDetector> TextDetectorFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(TextDetectorFactory.DetectorNames.ToArray());
        }
    }

    public class TextDetectorNameStringConverterWithDefault : StringConverter
    {
        [RequestStaticDependency(typeof(IFactoryWithCache<TextDetector>))]
        private static IFactoryWithCache<TextDetector> TextDetectorFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(TextDetectorFactory.DetectorNames.Append("Default").ToArray());
        }
    }
}
