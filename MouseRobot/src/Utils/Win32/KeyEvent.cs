using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Robot.Utils.Win32
{
    public enum KeyAction { KeyDown, KeyUp }

    public struct KeyEvent
    {
        public Keys keyCode;
        public KeyAction keyAction;
        private Point m_Point;

        public KeyEvent(Keys keyCode, KeyAction keyAction, Point point)
        {
            this.keyCode = keyCode;
            this.keyAction = keyAction;
            this.m_Point = point;
        }
        public KeyEvent(Keys keyCode, KeyAction keyAction)
        {
            this.keyCode = keyCode;
            this.keyAction = keyAction;
            this.m_Point = default(Point);
        }

        public bool IsKeyDown()
        {
            return KeyAction.KeyDown == keyAction;
        }

        public bool IsKeyUp()
        {
            return KeyAction.KeyUp == keyAction;
        }

        public int X { get { return m_Point.X; } }
        public int Y { get { return m_Point.Y; } }
    }
}
