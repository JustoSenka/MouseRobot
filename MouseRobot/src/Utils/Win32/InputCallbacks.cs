using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Robot.Utils.Win32
{
    public static class InputCallbacks
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private static readonly IntPtr WM_KEYDOWN = (IntPtr)0x0100;
        private static readonly IntPtr WM_KEYUP = (IntPtr)0x0101;

        //private static readonly IntPtr WM_MOUSEMOVE = (IntPtr)0x0200;
        private static readonly IntPtr WM_LBUTTONDOWN = (IntPtr)0x0201;
        private static readonly IntPtr WM_LBUTTONUP = (IntPtr)0x0202;
        private static readonly IntPtr WM_RBUTTONDOWN = (IntPtr)00204;
        private static readonly IntPtr WM_RBUTTONUP = (IntPtr)0x0205;
        //private static readonly IntPtr WM_MOUSEWHEEL = (IntPtr)0x020A;

        private static IntPtr m_KeyboardHook = IntPtr.Zero;
        private static IntPtr m_MouseHook = IntPtr.Zero;

        public static event Action<KeyEvent> inputEvent;

        static InputCallbacks()
        {
            m_KeyboardHook = SetHook(m_CachedLowLevelKeyboardProcDelegate, WH_KEYBOARD_LL);
            m_MouseHook = SetHook(m_CachedLowLevelMouseProcDelegate, WH_MOUSE_LL);
        }

        public static void ForgetAll()
        {
            if (inputEvent != null)
                foreach (var d in inputEvent.GetInvocationList())
                    inputEvent -= (d as Action<KeyEvent>);
        }

        private static readonly Destructor Finalise = new Destructor();
        private sealed class Destructor
        {
            ~Destructor()
            {
                UnhookWindowsHookEx(m_KeyboardHook);
                UnhookWindowsHookEx(m_MouseHook);
            }
        }

        private static IntPtr SetHook(LowLevelProcDelegate callback, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                var handle = GetModuleHandle(curModule.ModuleName);
                return SetWindowsHookEx(hookType, callback, handle, 0);
            }
        }

        private delegate IntPtr LowLevelProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        // We cache the delegate so it is not garbage collected. If we pass method directly, it creates delegate
        // out of it on spot and then lets it garbage collect if no reference. We keep ref in this class
        private static LowLevelProcDelegate m_CachedLowLevelKeyboardProcDelegate =
            new LowLevelProcDelegate(LowLevelKeyboardProcCallback);
        private static LowLevelProcDelegate m_CachedLowLevelMouseProcDelegate =
            new LowLevelProcDelegate(LowLevelMouseProcCallback);

        private static IntPtr LowLevelKeyboardProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam == WM_KEYDOWN || wParam == WM_KEYUP)
                {
                    var keyAction = wParam == WM_KEYDOWN ? KeyAction.KeyDown : KeyAction.KeyUp;
                    var key = (Keys)Marshal.ReadInt32(lParam);
                    inputEvent?.Invoke(new KeyEvent(key, keyAction, WinAPI.GetCursorPosition()));
                }
            return CallNextHookEx(m_KeyboardHook, nCode, wParam, lParam);
        }

        private static IntPtr LowLevelMouseProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam == WM_LBUTTONDOWN || wParam == WM_LBUTTONUP ||
                    wParam == WM_RBUTTONDOWN || wParam == WM_RBUTTONUP)
                {
                    var keyAction = (wParam == WM_RBUTTONDOWN || wParam == WM_LBUTTONDOWN) ? 
                        KeyAction.KeyDown : KeyAction.KeyUp;
                    var key = (wParam == WM_LBUTTONDOWN || wParam == WM_LBUTTONUP) ?
                        Keys.LButton : Keys.RButton;

                    var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    inputEvent?.Invoke(new KeyEvent(key, keyAction, new Point(hookStruct.pt.x, hookStruct.pt.y)));
                }
            return CallNextHookEx(m_KeyboardHook, nCode, wParam, lParam);
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProcDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
