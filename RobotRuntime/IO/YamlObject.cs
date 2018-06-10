using System.Text.RegularExpressions;

namespace RobotRuntime.IO
{
    public struct YamlObject
    {
        public short level;
        public string property;
        public string value;

        public YamlObject(int level, string property, object value)
        {
            this.level = (short)level;
            this.property = property;
            this.value = value.ToString();
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
            var matches = Regex.Match(YamlLine, @"^( *)([\W\w]+)(:[ ]?)([\w\W]*)$");
            level = (short)( matches.Groups[1].Value.Length / 2);
            property = matches.Groups[2].Value;
            value = matches.Groups.Count > 4 ? matches.Groups[4].Value : "";
        }

        public override string ToString()
        {
            return GetIndentation(level) + property + ": " + value;
        }

        public static string GetIndentation(int level)
        {
            var str = "";

            for (int i = 0; i < level; i++)
                str += "  ";

            return str;
        }
    }
}
