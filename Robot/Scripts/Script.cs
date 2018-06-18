using Robot.Abstractions;
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

        public event Action<Script, Command, Command> CommandAddedToScript;
        public event Action<Script, Command, Command, int> CommandInsertedInScript;
        public event Action<Script, Command, int> CommandRemovedFromScript;
        public event Action<Script, Command, Command> CommandModifiedOnScript;

        public string Name
        {
            get
            {
                return (Path == "") ? DefaultScriptName : Paths.GetName(Path);
            }
        }

        public int Index
        {
            get
            {
                return ScriptManager.LoadedScripts.IndexOf(this);
            }
        }

        public IScriptManager ScriptManager { get; set; }
        public Script()
        {
            Commands = new TreeNode<Command>();
        }

        public void ApplyCommandModifications(Command command)
        {
            m_IsDirty = true;
            CommandModifiedOnScript?.Invoke(this, command, command);
        }

        /// <summary>
        /// Adds non existant command to script bottom
        /// </summary>
        public Command AddCommand(Command command, Command parentCommand = null)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should not exist on script");

            m_IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.AddChild(command);
            CommandAddedToScript?.Invoke(this, parentCommand, command);
            return command;
        }

        /// <summary>
        /// Adds non existant command node to bottom of the script/parent with all its children.
        /// Calls CommandAdded event with root command of the node.
        /// </summary>
        public Command AddCommandNode(TreeNode<Command> commandNode, Command parentCommand = null)
        {
            Debug.Assert(!Commands.GetAllNodes().Contains(commandNode), "Command Node should not exist on script. Did you forget to remove it?");

            m_IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.Join(commandNode);
            CommandAddedToScript?.Invoke(this, parentCommand, commandNode.value);
            return commandNode.value;
        }

        /// <summary>
        /// Replace existing command value on its node. Will keep nested commands intact.
        /// Calls CommandModified event.
        /// </summary>
        public Command ReplaceCommand(Command originalCommand, Command newCommand)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(originalCommand), "Original Command should exist on script");
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(newCommand), "New Command should not exist on script");

            m_IsDirty = true;

            var node = Commands.GetNodeFromValue(originalCommand);
            node.value = newCommand;

            CommandModifiedOnScript?.Invoke(this, originalCommand, newCommand);
            return newCommand;
        }

        /// <summary>
        /// Insert single non existant command to specific location.
        /// </summary>
        public Command InsertCommand(Command command, int position, Command parentCommand = null)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should not exist on script");

            var treeNodeToInsert = (parentCommand == null) ? Commands : Commands.GetNodeFromValue(parentCommand);
            treeNodeToInsert.Insert(position, command);

            m_IsDirty = true;
            CommandInsertedInScript?.Invoke(this, parentCommand, command, position);
            return command;
        }

        /// <summary>
        /// Insert non existant command to specific location.
        /// Redirects call to InsertCommand(position)
        /// </summary>
        public Command InsertCommandAfter(Command sourceCommand, Command commandAfter)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(sourceCommand), "Source Command should not exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(commandAfter), "Destination Command should exist on script");

            var nodeAfter = Commands.GetNodeFromValue(commandAfter);
            var indexAfter = nodeAfter.parent.IndexOf(commandAfter);
            InsertCommand(sourceCommand, indexAfter + 1, nodeAfter.parent.value);

            m_IsDirty = true;
            return sourceCommand;
        }

        /// <summary>
        /// Moves an existing command from script to other place.
        /// Calls Removed and Inserted command events.
        /// </summary>
        public void MoveCommandAfter(Command source, Command after)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(source), "Source Command should exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(after), "Destination Command should exist on script");

            var oldIndex = source.GetIndex(ScriptManager);
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(after).parent.value;

            Commands.MoveAfter(source, after);

            CommandRemovedFromScript?.Invoke(this, sourceParentCommand, oldIndex);
            CommandInsertedInScript?.Invoke(this, destParentCommand, source, source.GetIndex(ScriptManager));
            m_IsDirty = true;
        }

        /// <summary>
        /// Moves an existing command from script to other place.
        /// Calls Removed and Inserted command events.
        /// </summary>
        public void MoveCommandBefore(Command source, Command before)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(source), "Source Command should exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(before), "Destination Command should exist on script");

            var oldIndex = source.GetIndex(ScriptManager);
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(before).parent.value;

            Commands.MoveBefore(source, before);

            CommandRemovedFromScript?.Invoke(this, sourceParentCommand, oldIndex);
            CommandInsertedInScript?.Invoke(this, destParentCommand, source, source.GetIndex(ScriptManager));
            m_IsDirty = true;
        }

        public void RemoveCommand(Command command)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should exist on script");

            var oldIndex = command.GetIndex(ScriptManager);
            var parentCommand = Commands.GetNodeFromValue(command).parent.value;

            Commands.Remove(command);

            CommandRemovedFromScript?.Invoke(this, parentCommand, oldIndex);
            m_IsDirty = true;
        }

        public object Clone()
        {
            var script = new Script();
            script.ScriptManager = ScriptManager;

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
                /*Debug.Assert(ScriptManager.IsTheCaller(),
                    "Only ScriptManager can change script path value. It should not be accessed from somewhere else");*/
                m_Path = value;
                IsDirty = false;
            }
            get { return m_Path; }
        }

        public bool IsDirty
        {
            set
            {
                /*Debug.Assert(ScriptManager.IsTheCaller(),
                    "Only ScriptManager can change script dirty value. It should not be accessed from somewhere else");*/

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
