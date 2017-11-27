using RobotRuntime;
using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Robot.Scripts
{
    public class Script : LightScript, ICloneable, IEnumerable<TreeNode<Command>>
    {
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
            Commands = new TreeNode<Command>();
        }

        public void ApplyCommandModifications(Command command)
        {
            m_IsDirty = true;
            ScriptManager.Instance.InvokeCommandModifiedOnScript(this, command);
        }

        public void AddCommand(Command command)
        {
            m_IsDirty = true;
            Commands.AddChild(command);
            ScriptManager.Instance.InvokeCommandAddedToScript(this, command);
        }

        public void ReplaceCommand(Command originalCommand, Command newCommand)
        {
            m_IsDirty = true;

            var index = Commands.IndexOf(originalCommand);
            Commands.RemoveAt(index);
            Commands.Insert(index, newCommand);

            ScriptManager.Instance.InvokeCommandModifiedOnScript(this, newCommand);
        }

        // Will not work in nested scenario
        public void InsertCommand(int position, Command command)
        {
            Commands.Insert(position, command);
            m_IsDirty = true;
            ScriptManager.Instance.InvokeCommandInsertedInScript(this, command, position);
        }

        // Will not work in nested scenario
        public void InsertCommandAfter(Command commandAfter, Command command)
        {
            if (commandAfter == null)
                InsertCommand(0, command);
            else
                InsertCommand(Commands.IndexOf(commandAfter) + 1, command);

            m_IsDirty = true;
        }

        // Will not work between different levels of nesting
        public void MoveCommandAfter(int index, int after)
        {
            var commandSource = Commands.GetChild(index).value;
            Commands.MoveAfter(commandSource.GetIndex(), after);

            ScriptManager.Instance.InvokeCommandRemovedFromScript(this, index);
            ScriptManager.Instance.InvokeCommandInsertedInScript(this, commandSource, commandSource.GetIndex());
            m_IsDirty = true;
        }

        // Will not work between different levels of nesting
        public void MoveCommandBefore(int index, int before)
        {
            var commandSource = Commands.GetChild(index).value;
            var commandAfter = Commands.GetChild(before).value;
            Commands.MoveBefore(index, before);

            ScriptManager.Instance.InvokeCommandRemovedFromScript(this, index);
            ScriptManager.Instance.InvokeCommandInsertedInScript(this, commandSource, commandSource.GetIndex());
            m_IsDirty = true;
        }

        // Will not work in nested scenario
        public void RemoveCommand(int index)
        {
            Commands.RemoveAt(index);
            m_IsDirty = true;
            ScriptManager.Instance.InvokeCommandRemovedFromScript(this, index);
        }

        // Will not work in nested scenario
        public void RemoveCommand(Command command)
        {
            var i = Commands.IndexOf(command);
            RemoveCommand(i);
        }

        public object Clone()
        {
            var script = new Script();

            script.Commands = (TreeNode<Command>)Commands.Clone();

            script.m_IsDirty = true;
            return script;
        }

        public override string ToString()
        {
            if (m_IsDirty)
                return Name + "*";
            else
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

        public Script(TreeNode<Command> commands) : base(commands) { Commands = commands; }

        public LightScript ToLightScript()
        {
            return new LightScript(Commands);
        }

        public Script(LightScript lightScript)
        {
            Commands = lightScript.Commands;
        }

        public static Script FromLightScript(LightScript lightScript)
        {
            return new Script(lightScript);
        }


        // IEnumerator -----------

        public IEnumerator<TreeNode<Command>> GetEnumerator()
        {
            return Commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Commands.GetEnumerator();
        }
    }
}
