﻿using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using WindowsInput;

namespace RobotRuntime.Utils.Win32
{
    public static class WinAPI
    {
        #region Mouse Input

        public const int TimeBetweenActions = 5;

        public static MouseEventFlags CurrentMouseState = MouseEventFlags.LeftUp | MouseEventFlags.MiddleUp | MouseEventFlags.RightUp;

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
            MouseMoveTo(position.X, position.Y);

            UpdateCurrentMouseState(value);
            mouse_event((int)value, position.X, position.Y, 0, 0);
            Thread.Sleep(TimeBetweenActions);
        }

        private static void UpdateCurrentMouseState(MouseEventFlags flag)
        {
            switch (flag)
            {
                case MouseEventFlags.LeftDown:
                    CurrentMouseState = (CurrentMouseState | MouseEventFlags.LeftDown) & ~MouseEventFlags.LeftUp;
                    break;
                case MouseEventFlags.LeftUp:
                    CurrentMouseState = (CurrentMouseState | MouseEventFlags.LeftUp) & ~MouseEventFlags.LeftDown;
                    break;
                case MouseEventFlags.MiddleDown:
                    CurrentMouseState = (CurrentMouseState | MouseEventFlags.MiddleDown) & ~MouseEventFlags.MiddleUp;
                    break;
                case MouseEventFlags.MiddleUp:
                    CurrentMouseState = (CurrentMouseState | MouseEventFlags.MiddleUp) & ~MouseEventFlags.MiddleDown;
                    break;
                case MouseEventFlags.RightDown:
                    CurrentMouseState = (CurrentMouseState | MouseEventFlags.RightDown) & ~MouseEventFlags.RightUp;
                    break;
                case MouseEventFlags.RightUp:
                    CurrentMouseState = (CurrentMouseState | MouseEventFlags.RightUp) & ~MouseEventFlags.RightDown;
                    break;
            }
        }

        public static void PerformActionDown(MouseButton value)
        {
            var flags = value == MouseButton.Left ? MouseEventFlags.LeftDown : 
                value == MouseButton.Right ? MouseEventFlags.RightDown :
                MouseEventFlags.MiddleDown;
            PerformAction(flags);
        }

        public static void PerformActionUp(MouseButton value)
        {
            var flags = value == MouseButton.Left ? MouseEventFlags.LeftUp :
                value == MouseButton.Right ? MouseEventFlags.RightUp :
                MouseEventFlags.MiddleUp;
            PerformAction(flags);
        }

        public static void MouseMoveTo(int x, int y)
        {
            SetCursorPosition(x, y);
            Thread.Sleep(TimeBetweenActions);
        }

        #endregion

        #region Keyboard Input

        private static InputSimulator InputSimulator = new InputSimulator();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        public static void SimulateTextEntry(string text)
        {
            InputSimulator.Keyboard.TextEntry(text);
        }

        #endregion

        #region Console show / hide

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

        #endregion
    }

    public enum MouseButton
    {
        Left, Right, Middle
    }
}
