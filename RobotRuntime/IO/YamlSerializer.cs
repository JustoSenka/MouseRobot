using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RobotRuntime.IO
{
    public static class YamlSerializer
    {
        public const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public static IEnumerable<YamlObject> SerializeSimpleProperties<T>(T objToWrite, int level)
        {
            if (objToWrite == null)
                yield break;

            var objType = objToWrite.GetType();
            var fields = objType.GetFields(k_BindingFlags);

            foreach (var f in fields)
            {
                var fieldType = f.FieldType;
                if (!IsSerializable(f))
                    continue;

                if (IsConvertibleType(fieldType))
                    yield return new YamlObject(level, f.Name, f.GetValue(objToWrite));
            }
        }

        public static void DeserializeSimpleProperties<T>(T objToWrite, TreeNode<YamlObject> tree)
        {
            if (objToWrite == null)
                return;

            foreach (var propNode in tree)
            {
                var field = objToWrite.GetType().GetField(propNode.value.property, k_BindingFlags);
                var fieldType = field != null ? field.FieldType : null;

                if (fieldType == null || !IsConvertibleType(fieldType))
                    continue;

                try
                {
                    var newVal = Convert.ChangeType(propNode.value.value, fieldType);
                    field.SetValue(objToWrite, newVal);
                }
                catch (Exception e)
                {
                    Logger.Log(LogType.Error, "Error while deserializing command. Failed to cast property '" + propNode.value.property + "' value to correct type", e.Message);
                }
            }
        }

        public static string SerializeYamlTree(TreeNode<YamlObject> tree)
        {
            var lines = tree.GetAllNodes().Select(node => node.value.ToString());
            return string.Join(Environment.NewLine, lines);
        }

        public static TreeNode<YamlObject> DeserializeYamlTree(string text)
        {
            var lines = text.Split('\n').Select(line => line.Trim('\n', '\r').TrimEnd(' ')).ToArray();

            var placeholderTree = new TreeNode<YamlObject>();

            var level = GetLevel(lines[0]);

            // Deserialize all root level elements
            for (int i = 0; i < lines.Length; ++i)
            {
                if (level == GetLevel(lines[i]))
                    DeserializeYamlLinesRecursively(placeholderTree, lines, i);
            }

            return placeholderTree.GetChild(0);
        }

        private static void DeserializeYamlLinesRecursively(TreeNode<YamlObject> root, string[] lines, int startIndex)
        {
            var level = GetLevel(lines[startIndex]);
            var yamlObject = new YamlObject(lines[startIndex]);
            var yamlObjectNode = root.AddChild(yamlObject);

            for (int i = startIndex + 1; i < lines.Length; ++i)
            {
                var currentLineLevel = GetLevel(lines[i]);
                if (level + 1 == currentLineLevel) // Adding childs to currently added yamlObject
                    DeserializeYamlLinesRecursively(yamlObjectNode, lines, i);
                else if (level + 1 < currentLineLevel)
                    continue;
                else if (level + 1 > currentLineLevel)
                    break;
            }
        }


        /*
        public static TreeNode<YamlObject> WriteAllPropertiesRecursively<T>(T objToWrite, int level)
        {
            if (objToWrite == null)
                return null;

            var objType = objToWrite.GetType();
            var fields = objType.GetFields(k_BindingFlags);

            foreach (var f in fields)
            {
                var fieldType = f.FieldType;
                if (!IsSerializable(f))
                    continue;

                if (IsEnumerable(fieldType))
                {
                    var list = f.GetValue(objToWrite) as IEnumerable;
                    if (list == null)
                        continue;

                    foreach (var val in list)
                        WriteAllPropertiesRecursively(builder, val, level + 1);

                }
                else if (!IsSimpleType(fieldType))
                {
                    var str = GetIndentation(level) + f.Name + ": ";

                    builder.AppendLine(str);
                    WriteAllPropertiesRecursively(builder, f.GetValue(objToWrite), level + 1);
                }
                else if (IsConvertibleType(fieldType))
                {
                    var str = GetIndentation(level) + f.Name + ": " + f.GetValue(objToWrite);
                    builder.AppendLine(str);
                }
            }
        }*/

        public static string GetIndentation(int level)
        {
            var str = "";

            for (int i = 0; i < level; i++)
                str += "  ";

            return str;
        }

        public static int GetLevel(string line)
        {
            var spaceString = Regex.Match(line, "^[ ]*").Value;
            return spaceString.Length / 2;
        }

        public static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                new Type[] { typeof(Enum), typeof(String), typeof(Decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
        }

        public static bool IsConvertibleType(Type type)
        {
            return typeof(IConvertible).IsAssignableFrom(type) || type == typeof(object);
        }

        public static bool IsSerializable(FieldInfo field)
        {
            return field.GetCustomAttributes().Count(a => a.GetType() == typeof(NonSerializedAttribute)) == 0;
        }

        public static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
