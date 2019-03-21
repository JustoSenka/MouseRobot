using System.Text.RegularExpressions;

namespace RobotRuntime.IO
{
    /// <summary>
    /// Is used to specify properties and values.
    /// Can take both property name or backing field name
    /// Can read both property name or backing field name
    /// Will only serialize property name, and remove obscure backing field text from it
    /// </summary>
    public struct YamlObject
    {
        public short level;
        public string property;
        public string value;

        public YamlObject(int level, string property, object value)
        {
            this.level = (short)level;
            this.value = value != null ? value.ToString() : "";
            this.property = property;

            this.property = RemoveBackingFieldText(property);
        }

        /// <summary>
        /// Constructor with one line of yaml text, some valid line examples:
        /// "  CommandForImage: "
        /// "    <Timeout>k__BackingField: 1850"
        /// Empty spaces in the beginning are important to determine level.
        /// Colon in the middle is important and should be there.
        /// Property value could be missing, but constructor should still create valid object with value as null.
        /// </summary>
        public YamlObject(string YamlLine)
        {
            var match = Regex.Match(YamlLine, @"^( *)([\W\w]+)(:[ ]?)([\w\W]*)$");
            level = (short)( match.Groups[1].Value.Length / 2);
            property = match.Groups[2].Value;
            value = match.Groups.Count > 4 ? match.Groups[4].Value : "";

            this.property = RemoveBackingFieldText(property);
        }

        public override string ToString()
        {
            var p = RemoveBackingFieldText(property);
            return GetIndentation(level) + p + ": " + value;
        }

        public static string GetIndentation(int level)
        {
            var str = "";

            for (int i = 0; i < level; i++)
                str += "  ";

            return str;
        }

        public static string RemoveBackingFieldText(string prop)
        {
            var p = Regex.Match(prop, @"<(\w+)>k__BackingField")?.Groups?[1].Value;
            return string.IsNullOrEmpty(p) ? prop : p;
        }

        public static string AddBackingFieldText(string prop)
        {
            return prop.Contains("k__BackingField") ? prop : $"<{prop}>k__BackingField";
        }
    }
}
