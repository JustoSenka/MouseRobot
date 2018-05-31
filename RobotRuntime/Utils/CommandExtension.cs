using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotRuntime.Utils
{
    public static class CommandExtension
    {
        public static void CopyPropertyFromIfExist(this Command dest, Command source, string name)
        {
            var destProp = dest.GetType().GetProperty(name);
            var sourceProp = source.GetType().GetProperty(name);

            if (destProp != null && sourceProp != null)
            {
                destProp.SetValue(dest, sourceProp.GetValue(source));
            }
        }

        public static void SetPropertyIfExist(this Command dest, string name, object value)
        {
            var destProp = dest.GetType().GetProperty(name);
            destProp?.SetValue(dest, value);
        }

        public static object GetPropertyIfExist(this Command source, string name)
        {
            var prop = source.GetType().GetProperty(name);
            return prop != null ? prop.GetValue(source) : null;
        }
    }
}
