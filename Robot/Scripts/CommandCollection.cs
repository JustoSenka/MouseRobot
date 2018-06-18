using RobotRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Robot.Scripts
{
    public class CommandCollection : LightScript, ICloneable, IEnumerable<TreeNode<Command>>
    {
        public event Action<CommandCollection> DirtyChanged;

        public string Name { get;  private set;}
        public int Index { get { return AbstractCommandCollectionManager != null ? AbstractCommandCollectionManager.LoadedScripts.IndexOf(this) : 0; } }

        private bool m_IsDirty;
        public bool IsDirty
        {
            set
            {
                if (m_IsDirty != value)
                    DirtyChanged?.Invoke(this);

                m_IsDirty = value;
            }
            get { return m_IsDirty; }
        } 

        public AbstractCommandCollectionManager AbstractCommandCollectionManager { get; set; }
        public CommandCollection()
        {
            Commands = new TreeNode<Command>();
        }

        public void ApplyCommandModifications(Command command)
        {
            IsDirty = true;
            AbstractCommandCollectionManager?.InvokeCommandModifiedOnScript(this, command, command);
        }

        /// <summary>
        /// Adds non existant command to script bottom
        /// </summary>
        public Command AddCommand(Command command, Command parentCommand = null)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should not exist on script");

            IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.AddChild(command);
            AbstractCommandCollectionManager?.InvokeCommandAddedToScript(this, parentCommand, command);
            return command;
        }

        /// <summary>
        /// Adds non existant command node to bottom of the script/parent with all its children.
        /// Calls CommandAdded event with root command of the node.
        /// </summary>
        public Command AddCommandNode(TreeNode<Command> commandNode, Command parentCommand = null)
        {
            Debug.Assert(!Commands.GetAllNodes().Contains(commandNode), "Command Node should not exist on script. Did you forget to remove it?");

            IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.Join(commandNode);
            AbstractCommandCollectionManager?.InvokeCommandAddedToScript(this, parentCommand, commandNode.value);
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

            IsDirty = true;

            var node = Commands.GetNodeFromValue(originalCommand);
            node.value = newCommand;

            AbstractCommandCollectionManager?.InvokeCommandModifiedOnScript(this, originalCommand, newCommand);
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

            IsDirty = true;
            AbstractCommandCollectionManager?.InvokeCommandInsertedInScript(this, parentCommand, command, position);
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

            IsDirty = true;
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

            var oldIndex = source.GetIndex(AbstractCommandCollectionManager);
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(after).parent.value;

            Commands.MoveAfter(source, after);

            AbstractCommandCollectionManager?.InvokeCommandRemovedFromScript(this, sourceParentCommand, oldIndex);
            AbstractCommandCollectionManager?.InvokeCommandInsertedInScript(this, destParentCommand, source, source.GetIndex(AbstractCommandCollectionManager));
            IsDirty = true;
        }

        /// <summary>
        /// Moves an existing command from script to other place.
        /// Calls Removed and Inserted command events.
        /// </summary>
        public void MoveCommandBefore(Command source, Command before)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(source), "Source Command should exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(before), "Destination Command should exist on script");

            var oldIndex = source.GetIndex(AbstractCommandCollectionManager);
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(before).parent.value;

            Commands.MoveBefore(source, before);

            AbstractCommandCollectionManager?.InvokeCommandRemovedFromScript(this, sourceParentCommand, oldIndex);
            AbstractCommandCollectionManager?.InvokeCommandInsertedInScript(this, destParentCommand, source, source.GetIndex(AbstractCommandCollectionManager));
            IsDirty = true;
        }

        public void RemoveCommand(Command command)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should exist on script");

            var oldIndex = command.GetIndex(AbstractCommandCollectionManager);
            var parentCommand = Commands.GetNodeFromValue(command).parent.value;

            Commands.Remove(command);

            AbstractCommandCollectionManager?.InvokeCommandRemovedFromScript(this, parentCommand, oldIndex);
            IsDirty = true;
        }

        public object Clone()
        {
            var script = new CommandCollection();
            script.AbstractCommandCollectionManager = AbstractCommandCollectionManager;

            script.Commands = (TreeNode<Command>)Commands.Clone();

            script.IsDirty = true;
            return script;
        }

        // Inheritence

        public CommandCollection(TreeNode<Command> commands) : base(commands) { Commands = commands; }

        public LightScript ToLightScript()
        {
            return new LightScript(Commands);
        }

        public CommandCollection(LightScript lightScript)
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
