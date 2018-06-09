using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public static string SerializeYamlTree(TreeNode<YamlObject> tree)
        {
            var lines = tree.GetAllNodes().Select(node => node.value.ToString());
            return string.Join(Environment.NewLine, lines); 
        }
        /*
        public static TreeNode<YamlObject> WriteAllPropertiesRecursivelly<T>(T objToWrite, int level)
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
                        WriteAllPropertiesRecursivelly(builder, val, level + 1);

                }
                else if (!IsSimpleType(fieldType))
                {
                    var str = GetIndentation(level) + f.Name + ": ";

                    builder.AppendLine(str);
                    WriteAllPropertiesRecursivelly(builder, f.GetValue(objToWrite), level + 1);
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
