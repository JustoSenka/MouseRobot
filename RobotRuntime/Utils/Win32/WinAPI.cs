using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;

namespace RobotRuntime.Utils.Win32
{
    public static class WinAPI
    {
        public static readonly int TimeBetweenActions = 5;

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Point lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public static void SetCursorPosition(int X, int Y)
        {
            SetCursorPos(X, Y);
        }

        public static void SetCursorPosition(Point point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static Point GetCursorPosition()
        {
            Point currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new Point(0, 0); }
            return currentMousePoint;
        }

        public static void PerformAction(MouseEventFlags value)
        {
            Point position = GetCursorPosition();
            mouse_event((int)value, position.X, position.Y, 0, 0);
            Thread.Sleep(TimeBetweenActions);
        }

        public static void MouseMoveTo(int x, int y)
        {
            SetCursorPosition(x, y);
            Thread.Sleep(TimeBetweenActions);
        }

        
        // Console show / hide

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static readonly int SW_HIDE = 0;
        public static readonly int SW_SHOW = 5;

        internal static IntPtr GetConsole()
        {
            return GetConsoleWindow();
        }

        internal static bool Show(IntPtr hWnd, int nCmdShow)
        {
            return ShowWindow(hWnd, nCmdShow);
        }
    }
}
