using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using Robot.Utils.Win32;

namespace Robot
{
    [Serializable]
    public class Script : ICloneable, IEnumerable<Command>
    {
        private IList<Command> m_Commands;
        public IReadOnlyList<Command> Commands { get { return m_Commands.ToList().AsReadOnly(); } }

        [NonSerialized]
        private bool m_IsDirty;
        [NonSerialized]
        private string m_Path;

        [NonSerialized]
        public bool dontSendMessagesToManager;

        public event Action<Script> DirtyChanged;

        public string Name
        {
            get
            {
                return (Path == "") ? "New Script" : Regex.Match(Path, RegexExpression.GetScriptNameFromPath).Value.Trim('/', '\\').Replace(FileExtensions.ScriptD, "");
            }
        }

        public int Index
        {
            get
            {
                return ScriptManager.Instance.LoadedScripts.IndexOf(this);
            }
        }

        public Script()
        {
            m_Commands = new List<Command>();
            Path = "";
        }

        public void AddCommandSleep(int time)
        {
            m_IsDirty = true;
            var command = new Command(() =>
            {
                Thread.Sleep(time);
                CheckIfPointerOffScreen();
            }, "Sleep for " + time + " ms.", CommandCode.G, time);

            m_Commands.Add(command);

            if (!dontSendMessagesToManager)
                ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandRelease()
        {
            m_IsDirty = true;
            var command = new Command(() =>
            {
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
                CheckIfPointerOffScreen();
            }, "Release", CommandCode.K);

            m_Commands.Add(command);

            if (!dontSendMessagesToManager)
                ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandPress(int x, int y)
        {
            m_IsDirty = true;
            var command = new Command(delegate ()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
                CheckIfPointerOffScreen();
            }, "Press on: (" + x + ", " + y + ")", CommandCode.S, x, y);

            m_Commands.Add(command);

            if (!dontSendMessagesToManager)
                ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandMove(int x, int y)
        {
            m_IsDirty = true;
            var command = new Command(delegate ()
            {
                int x1, y1;
                x1 = WinAPI.GetCursorPosition().X;
                y1 = WinAPI.GetCursorPosition().Y;

                for (int i = 1; i <= 50; i++)
                {
                    MouseMoveTo(x1 + ((x - x1) * i / 50), y1 + ((y - y1) * i / 50));
                }
                CheckIfPointerOffScreen();
            }, "Move to: (" + x + ", " + y + ")", CommandCode.J, x, y);

            m_Commands.Add(command);

            if (!dontSendMessagesToManager)
                ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandDown(int x, int y)
        {
            m_IsDirty = true;
            var command = new Command(delegate ()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                CheckIfPointerOffScreen();
            }, "Down on: (" + x + ", " + y + ")", CommandCode.H, x, y);

            m_Commands.Add(command);

            if (!dontSendMessagesToManager)
                ScriptManager.Instance.ScriptModified(this, command);
        }

        public void InsertCommand(int position, Command command)
        {
            m_Commands.Insert(position, command);
            m_IsDirty = true;
        }

        public void MoveCommandAfter(int index, int after)
        {
            m_Commands.MoveAfter(index, after);
            m_IsDirty = true;
        }

        public void MoveCommandBefore(int index, int before)
        {
            m_Commands.MoveBefore(index, before);
            m_IsDirty = true;
        }

        public void RemoveCommand(int index)
        {
            m_Commands.RemoveAt(index);
            m_IsDirty = true;
        }
    
        public void EmptyScript()
        {
            m_Commands.Clear();
            m_IsDirty = true;
            Console.WriteLine("List is empty.");
        }

        private Action<int, int> MouseMoveTo = (int x, int y) =>
        {
            WinAPI.SetCursorPosition(x, y);
            Thread.Sleep(WinAPI.TimeBetweenActions);
        };

        private Action<WinAPI.MouseEventFlags> MouseAction = (WinAPI.MouseEventFlags flags) =>
        {
            WinAPI.MouseEvent(flags);
            Thread.Sleep(WinAPI.TimeBetweenActions);
        };

        private bool CheckIfPointerOffScreen()
        {
            if (WinAPI.GetCursorPosition().Y < 5)
            {
                // Stop the script here
            }
            return WinAPI.GetCursorPosition().Y < 5;
        }

        public object Clone()
        {
            var script = new Script();
            script.dontSendMessagesToManager = true;

            foreach (var v in m_Commands)
            {
                switch (v.Code)
                {
                    case CommandCode.G:
                        script.AddCommandSleep(v.Args.ElementAt(0));
                        break;
                    case CommandCode.S:
                        script.AddCommandPress(v.Args.ElementAt(0), v.Args.ElementAt(1));
                        break;
                    case CommandCode.H:
                        script.AddCommandDown(v.Args.ElementAt(0), v.Args.ElementAt(1));
                        break;
                    case CommandCode.J:
                        script.AddCommandMove(v.Args.ElementAt(0), v.Args.ElementAt(1));
                        break;
                    case CommandCode.K:
                        script.AddCommandRelease();
                        break;
                }
            }

            script.dontSendMessagesToManager = false;
            script.m_IsDirty = true;
            return script;
        }

        public override string ToString()
        {
            return Name;
        }

        // Special properties
        public string Path
        {
            set
            {
                Debug.Assert(ScriptManager.Instance.IsTheCaller(),
                    "Only ScriptManager can change script path value. It should not be accessed from somewhere else");
                m_Path = value;
                IsDirty = false;
            }
            get { return m_Path; }
        }

        public bool IsDirty
        {
            set
            {
                Debug.Assert(ScriptManager.Instance.IsTheCaller(),
                    "Only ScriptManager can change script dirty value. It should not be accessed from somewhere else");

                if (m_IsDirty != value)
                    DirtyChanged?.Invoke(this);

                m_IsDirty = value;
            }
            get { return m_IsDirty; }
        }


        // IEnumerator -----------

        public IEnumerator<Command> GetEnumerator()
        {
            return m_Commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Commands.GetEnumerator();
        }
    }
}
