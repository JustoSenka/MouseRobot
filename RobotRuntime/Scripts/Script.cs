using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using RobotRuntime.Commands;

namespace RobotRuntime
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

        public event Action<Script> DirtyChanged;

        public string Name
        {
            get
            {
                return (Path == "") ? "New Script" : Commons.GetName(Path);
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
            var command = new CommandSleep(time);
            m_Commands.Add(command);

            ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandRelease()
        {
            m_IsDirty = true;
            var command = new CommandRelease();
            m_Commands.Add(command);

            ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandPress(int x, int y)
        {
            m_IsDirty = true;
            var command = new CommandPress(x, y);
            m_Commands.Add(command);

            ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandMove(int x, int y)
        {
            m_IsDirty = true;
            var command = new CommandMove(x, y);
            m_Commands.Add(command);

            ScriptManager.Instance.ScriptModified(this, command);
        }

        public void AddCommandDown(int x, int y)
        {
            m_IsDirty = true;
            var command = new CommandDown(x, y);
            m_Commands.Add(command);

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

        public object Clone()
        {
            var script = new Script();

            foreach (var c in m_Commands)
                script.m_Commands.Add((Command)c.Clone());

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
