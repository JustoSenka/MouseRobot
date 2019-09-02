using Microsoft.Win32;
using RobotRuntime;
using Robot.Abstractions;
using System;

namespace Robot.Utils
{
    [RegisterTypeToContainer(typeof(IRegistryEditor))]
    public class RegistryEditor : IRegistryEditor
    {
        private readonly string m_AppRoot;

        public RegistryEditor()
        {
            m_AppRoot = Properties.Resources.RegistryRoot;
        }

        public void Put(string key, object value)
        {
            try
            {
                using (var k = OpenOrCreateKey(m_AppRoot, true))
                {
                    k.SetValue(key, value);
                    k.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot put value into registry: " + key, e.Message);
            }
        }

        public object Get(string key)
        {
            try
            {
                using (var k = OpenOrCreateKey(m_AppRoot, false))
                {
                    var val = k.GetValue(key);
                    k.Close();
                    return val;
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot get values from registry: " + key, e.Message);
                return null;
            }
        }

        private RegistryKey OpenOrCreateKey(string rootKey, bool writable)
        {
            try
            {
                var k = Registry.CurrentUser.OpenSubKey(rootKey, writable);

                if (k == null)
                    k = Registry.CurrentUser.CreateSubKey(rootKey, writable);

                return k;
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot create sub key in registry: " + rootKey, e.Message);
                return null;
            }
        }
    }
}
