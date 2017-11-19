using Robot.Utils;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Robot
{
    public class Script : LightScript, ICloneable, IEnumerable<TreeNode<Command>>
    {
        private TreeNode<Command> m_Commands;
        public new IReadOnlyList<Command> Commands { get { return m_Commands.Select(node => node.value).ToList().AsReadOnly(); } }

        private bool m_IsDirty;
        private string m_Path = "";

        public event Action<Script> DirtyChanged;
        public const string DefaultScriptName = "New Script";

        public string Name
        {
            get
            {
                return (Path == "") ? DefaultScriptName : Commons.GetName(Path);
            }
        }

        public int Index
        {
            get
            {
                return ScriptManager.Instance.LoadedScripts.IndexOf(this);
            }
        }

        internal Script()
        {
            m_Commands = new TreeNode<Command>();
        }

        public void ApplyCommandModifications(Command command)
        {
            m_IsDirty = true;
            ScriptManager.Instance.InvokeCommandModifiedOnScript(this, command);
        }

        public void AddCommand(Command command)
        {
            m_IsDirty = true;
            m_Commands.AddChild(command);
            ScriptManager.Instance.InvokeCommandAddedToScript(this, command);
        }

        public void ReplaceCommand(Command originalCommand, Command newCommand)
        {
            m_IsDirty = true;

            var index = m_Commands.IndexOf(originalCommand);
            m_Commands.RemoveAt(index);
            m_Commands.Insert(index, newCommand);

            ScriptManager.Instance.InvokeCommandModifiedOnScript(this, newCommand);
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

        public object Clone()
        {
            var script = new Script();

            script.m_Commands = (TreeNode<Command>)m_Commands.Clone();

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

        // Inheritence

        public Script(TreeNode<Command> commands) : base(commands) { m_Commands = commands; }

        public LightScript ToLightScript()
        {
            return new LightScript(m_Commands);
        }

        public Script(LightScript lightScript)
        {
            m_Commands = lightScript.Commands;
        }

        public static Script FromLightScript(LightScript lightScript)
        {
            return new Script(lightScript);
        }


        // IEnumerator -----------

        public IEnumerator<TreeNode<Command>> GetEnumerator()
        {
            return m_Commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Commands.GetEnumerator();
        }
    }
}
