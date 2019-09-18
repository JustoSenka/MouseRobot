using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using RobotRuntime.Utils.Win32;
using System.Threading;
using System.ComponentModel;
using Robot.Abstractions;
using RobotRuntime;
using Unity.Lifetime;

namespace Robot.Input
{
    [RegisterTypeToContainer(typeof(IInputCallbacks), typeof(ContainerControlledLifetimeManager))]
    public class InputCallbacks : IInputCallbacks
    {
        public AsyncOperation AsyncOperationOnUI { private get; set; }

        public event Action<KeyEvent> InputEvent;

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private static readonly IntPtr WM_KEYDOWN = (IntPtr)0x0100;
        private static readonly IntPtr WM_KEYUP = (IntPtr)0x0101;

        //private static readonly IntPtr WM_MOUSEMOVE = (IntPtr)0x0200;
        private static readonly IntPtr WM_LBUTTONDOWN = (IntPtr)0x0201;
        private static readonly IntPtr WM_LBUTTONUP = (IntPtr)0x0202;
        private static readonly IntPtr WM_RBUTTONDOWN = (IntPtr)0x0204;
        private static readonly IntPtr WM_RBUTTONUP = (IntPtr)0x0205;
        private static readonly IntPtr WM_MBUTTONDOWN = (IntPtr)0x0207;
        private static readonly IntPtr WM_MBUTTONUP = (IntPtr)0x0208;
        //private static readonly IntPtr WM_MOUSEWHEEL = (IntPtr)0x020A;

        private static IntPtr m_KeyboardHook = IntPtr.Zero;
        private static IntPtr m_MouseHook = IntPtr.Zero;

        private static bool m_Init = false;

        public InputCallbacks() { }

        public void Init()
        {
            if (m_Init)
                return;

            m_Init = true;
            new Thread(() =>
            {
                m_CachedLowLevelKeyboardProcDelegate = new LowLevelProcDelegate(LowLevelKeyboardProcCallback);
                m_CachedLowLevelMouseProcDelegate = new LowLevelProcDelegate(LowLevelMouseProcCallback);

                m_KeyboardHook = SetHook(m_CachedLowLevelKeyboardProcDelegate, WH_KEYBOARD_LL);
                m_MouseHook = SetHook(m_CachedLowLevelMouseProcDelegate, WH_MOUSE_LL);
                Application.Run(); // TODO: Find a better way to implement Application Message Loop.... LOL
            }).Start();
        }

        public void ForgetAll()
        {
            if (InputEvent != null)
                foreach (var d in InputEvent.GetInvocationList())
                    InputEvent -= (d as Action<KeyEvent>);

            InputEvent = null;
        }

        // We cache the delegate so it is not garbage collected. If we pass method directly, it creates delegate
        // out of it on spot and then lets it garbage collect if no reference. We keep ref in this class
        private LowLevelProcDelegate m_CachedLowLevelKeyboardProcDelegate;
        private LowLevelProcDelegate m_CachedLowLevelMouseProcDelegate;
        private delegate IntPtr LowLevelProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr LowLevelKeyboardProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam == WM_KEYDOWN || wParam == WM_KEYUP)
                {
                    var keyAction = wParam == WM_KEYDOWN ? KeyAction.KeyDown : KeyAction.KeyUp;
                    var key = (Keys)Marshal.ReadInt32(lParam);
                    InvokeEventOnAsyncOperation(new KeyEvent(key, keyAction, WinAPI.GetCursorPosition()));
                }
            return CallNextHookEx(m_KeyboardHook, nCode, wParam, lParam);
        }

        private IntPtr LowLevelMouseProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam == WM_LBUTTONDOWN || wParam == WM_LBUTTONUP ||
                    wParam == WM_RBUTTONDOWN || wParam == WM_RBUTTONUP ||
                    wParam == WM_MBUTTONDOWN || wParam == WM_MBUTTONUP )
                {
                    var keyAction = (wParam == WM_RBUTTONDOWN || wParam == WM_LBUTTONDOWN || wParam == WM_MBUTTONDOWN) ?
                        KeyAction.KeyDown : KeyAction.KeyUp;
                    var key = (wParam == WM_LBUTTONDOWN || wParam == WM_LBUTTONUP) ? Keys.LButton :
                        (wParam == WM_RBUTTONDOWN || wParam == WM_RBUTTONUP) ? Keys.RButton :
                        Keys.MButton;

                    var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    InvokeEventOnAsyncOperation(new KeyEvent(key, keyAction, new Point(hookStruct.pt.x, hookStruct.pt.y)));
                }
            return CallNextHookEx(m_KeyboardHook, nCode, wParam, lParam);
        }

        private void InvokeEventOnAsyncOperation(KeyEvent e)
        {
            if (InputEvent == null || AsyncOperationOnUI == null)
                return;

            AsyncOperationOnUI.Post(new SendOrPostCallback(delegate (object state)
            {
                InputEvent?.Invoke(e);
            }), null);

            //AsyncOperation.OperationCompleted();
        }

        ~InputCallbacks()
        {
            UnhookWindowsHookEx(m_KeyboardHook);
            UnhookWindowsHookEx(m_MouseHook);
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
