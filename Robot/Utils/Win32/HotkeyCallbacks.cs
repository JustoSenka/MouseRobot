using Robot.Abstractions;
using RobotRuntime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Unity.Lifetime;

namespace Robot.Utils.Win32
{
    [RegisterTypeToContainer(typeof(IHotkeyCallbacks), typeof(ContainerControlledLifetimeManager))]
    public class HotkeyCallbacks : IHotkeyCallbacks
    {
        public Dictionary<Keys, Action> Hotkeys { get; } = new Dictionary<Keys, Action>();

        private IntPtr hWnd;

        private const int WM_HOTKEY_MSG_ID = 0x0312;
        private const int NOMOD = 0x0000;
        private const int ALT = 0x0001;
        private const int CTRL = 0x0002;
        private const int SHIFT = 0x0004;
        private const int WIN = 0x0008;

        public HotkeyCallbacks() { }
        ~HotkeyCallbacks()
        {
            try
            {
                UnregisterAll();
            }
            catch { }
        }

        public void RegisterForm(Form form) => hWnd = form.Handle;

        public bool ProcessCallback(Message m)
        {
            if (m.Msg == WM_HOTKEY_MSG_ID)
            {
                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                //int modifier = (int)m.LParam & 0xFFFF;

                Keys key = (Keys)((int)m.LParam >> 16);

                if (Hotkeys.ContainsKey(key))
                {
                    Hotkeys[key].Invoke();
                    return true;
                }

                return false;
            }
            return false;
        }

        public int GetHashCode(Keys key)
        {
            return ((int)key) ^ hWnd.ToInt32();
        }

        public bool Register(Keys key, Action callback)
        {
            if (!Hotkeys.ContainsKey(key))
            {
                Hotkeys.Add(key, callback);
                return RegisterHotKey(hWnd, GetHashCode(key), 0, (int)key);
            }
            return false;
        }

        public bool Unregiser(Keys key)
        {
            if (Hotkeys.ContainsKey(key))
            {
                Hotkeys.Remove(key);
                return UnregisterHotKey(hWnd, GetHashCode(key));
            }
            return false;
        }

        private void UnregisterAll()
        {
            foreach (var key in Hotkeys.Keys)
                Unregiser(key);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
