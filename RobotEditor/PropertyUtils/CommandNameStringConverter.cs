using Robot.Abstractions;
using System.ComponentModel;
using System.Linq;

namespace RobotEditor.PropertyUtils
{
    public class CommandNameStringConverter : StringConverter
    {
        private static ICommandFactory CommandFactory { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(CommandFactory.CommandNames.ToArray());
        }
    }
}
