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
            ScriptManager.Instance.InvokeCommandModifiedOnScript(this, command, command);
        }

        public Command AddCommand(Command command, Command parentCommand = null)
        {
            m_IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.AddChild(command);
            ScriptManager.Instance.InvokeCommandAddedToScript(this, parentCommand, command);
            return command;
        }

        public Command AddCommandNode(TreeNode<Command> commandNode, Command parentCommand = null)
        {
            m_IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.Join(commandNode);
            ScriptManager.Instance.InvokeCommandAddedToScript(this, parentCommand, commandNode.value);
            return commandNode.value;
        }

        public Command ReplaceCommand(Command originalCommand, Command newCommand)
        {
            m_IsDirty = true;

            var node = Commands.GetNodeFromValue(originalCommand);
            node.value = newCommand;

            ScriptManager.Instance.InvokeCommandModifiedOnScript(this, originalCommand, newCommand);
            return newCommand;
        }

        public Command InsertCommand(Command command, int position, Command parentCommand = null)
        {
            var treeNodeToInsert = (parentCommand == null) ? Commands : Commands.GetNodeFromValue(parentCommand);
            treeNodeToInsert.Insert(position, command);

            m_IsDirty = true;
            ScriptManager.Instance.InvokeCommandInsertedInScript(this, parentCommand, command, position);
            return command;
        }

        public Command InsertCommandAfter(Command sourceCommand, Command commandAfter)
        {
            var nodeAfter = Commands.GetNodeFromValue(commandAfter);
            var indexAfter = nodeAfter.parent.IndexOf(commandAfter);
            InsertCommand(sourceCommand, indexAfter, nodeAfter.parent.value);

            m_IsDirty = true;
            return sourceCommand;
        }

        public void MoveCommandAfter(Command source, Command after)
        {
            var oldIndex = source.GetIndex();
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(after).parent.value;

            Commands.MoveAfter(source, after);

            ScriptManager.Instance.InvokeCommandRemovedFromScript(this, sourceParentCommand, oldIndex);
            ScriptManager.Instance.InvokeCommandInsertedInScript(this, destParentCommand, source, source.GetIndex());
            m_IsDirty = true;
        }

        public void MoveCommandBefore(Command source, Command before)
        {
            var oldIndex = source.GetIndex();
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(before).parent.value;

            Commands.MoveBefore(source, before);

            ScriptManager.Instance.InvokeCommandRemovedFromScript(this, sourceParentCommand, oldIndex);
            ScriptManager.Instance.InvokeCommandInsertedInScript(this, destParentCommand, source, source.GetIndex());
            m_IsDirty = true;
        }

        public void RemoveCommand(Command command)
        {
            var oldIndex = command.GetIndex();
            var parentCommand = Commands.GetNodeFromValue(command).parent.value;

            Commands.Remove(command);

            ScriptManager.Instance.InvokeCommandRemovedFromScript(this, parentCommand, oldIndex);
            m_IsDirty = true;
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
