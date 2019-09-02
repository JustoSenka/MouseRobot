using System;
using System.Reflection;

namespace RobotRuntime.Reflection
{
    public static class ReflectionExtension
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public static void CopyAllProperties<T>(this T dest, T source) where T : class
        {
            var destProps = dest.GetType().GetProperties(k_BindingFlags);
            var sourceProps = source.GetType().GetProperties(k_BindingFlags);

            foreach(var destProp in destProps)
            {
                foreach(var sourceProp in sourceProps)
                {
                    if (destProp.CanWrite && sourceProp.CanRead && destProp.PropertyType == sourceProp.PropertyType && destProp.Name == sourceProp.Name)
                        destProp.SetValue(dest, sourceProp.GetValue(source));
                }
            }
        }

        public static void CopyAllFields<T>(this T dest, T source) where T : class
        {
            var destFields = dest.GetType().GetFields(k_BindingFlags);
            var sourceFields = source.GetType().GetFields(k_BindingFlags);

            foreach (var destField in destFields)
            {
                foreach (var sourceField in sourceFields)
                {
                    if (destField.FieldType == sourceField.FieldType && destField.Name == sourceField.Name)
                        destField.SetValue(dest, sourceField.GetValue(source));
                }
            }
        }

        public static void CopyPropertyFromIfExist<T>(this T dest, T source, string name) where T : class
        {
            var destProp = dest.GetType().GetProperty(name, k_BindingFlags);
            var sourceProp = source.GetType().GetProperty(name, k_BindingFlags);

            if (destProp != null && sourceProp != null)
            {
                destProp.SetValue(dest, sourceProp.GetValue(source));
            }
        }

        public static void SetPropertyIfExist<T>(this T dest, string name, object value) where T : class
        {
            var destProp = dest.GetType().GetProperty(name, k_BindingFlags);
            destProp?.SetValue(dest, value);
        }

        public static object GetPropertyIfExist<T>(this T source, string name) where T : class
        {
            var prop = source.GetType().GetProperty(name, k_BindingFlags);
            return prop != null ? prop.GetValue(source) : null;
        }

        public static void SetFieldIfExist<T>(this T dest, string name, object value) where T : class
        {
            var destField = dest.GetType().GetField(name, k_BindingFlags);
            destField?.SetValue(dest, value);
        }

        public static object GetFieldIfExist<T>(this T source, string name) where T : class
        {
            var field = source.GetType().GetField(name, k_BindingFlags);
            return field != null ? field.GetValue(source) : null;
        }
    }
}
