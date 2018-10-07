﻿using RobotRuntime.Execution;
using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RobotRuntime.Scripts
{
    [PropertyDesignerType("ScriptProperties")]
    public class Script : LightScript, ICloneable, IEnumerable<TreeNode<Command>>, ISimilar, IHaveGuid, IHaveGuidMap
    {
        private bool m_IsDirty;
        private string m_Path = "";

        public event Action<Script> DirtyChanged;
        public const string DefaultScriptName = "New Script";

        public event Action<Script, Command, Command> CommandAddedToScript;
        public event Action<Script, Command, Command, int> CommandInsertedInScript;
        public event Action<Script, Command, int> CommandRemovedFromScript;
        public event Action<Script, Command, Command> CommandModifiedOnScript;

        protected readonly HashSet<Guid> CommandGuidMap = new HashSet<Guid>();

        public Script(Guid guid = default(Guid)) : base(guid)
        {
            Commands = new TreeNode<Command>();
        }

        public void ApplyCommandModifications(Command command)
        {
            m_IsDirty = true;
            CommandModifiedOnScript?.Invoke(this, command, command);
        }

        /// <summary>
        /// Adds non existant single command to the bottom of the script. Does not add command children.
        /// </summary>
        public Command AddCommand(Command command, Command parentCommand = null)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should not exist on script");

            m_IsDirty = true;

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(command);

            nodeToAddCommand.AddChild(command);
            CommandAddedToScript?.Invoke(this, parentCommand, command);

            DEBUG_CheckCommandGuidConsistency();
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

            foreach (var node in commandNode.GetAllNodes(true))
                CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(node.value);

            var nodeToAddCommand = parentCommand == null ? Commands : Commands.GetNodeFromValue(parentCommand);

            nodeToAddCommand.Join(commandNode);

            CommandAddedToScript?.Invoke(this, parentCommand, commandNode.value);

            DEBUG_CheckCommandGuidConsistency();
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

            CommandGuidMap.RemoveGuidFromMap(node.value);
            CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(newCommand);

            node.value = newCommand;

            CommandModifiedOnScript?.Invoke(this, originalCommand, newCommand);

            DEBUG_CheckCommandGuidConsistency();
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

            CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(command);

            m_IsDirty = true;
            CommandInsertedInScript?.Invoke(this, parentCommand, command, position);

            DEBUG_CheckCommandGuidConsistency();
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
            return InsertCommand(sourceCommand, indexAfter + 1, nodeAfter.parent.value);
        }

        /// <summary>
        /// Inserts non existant command node after specified command with all its children.
        /// Calls CommandInserted event
        /// </summary>
        public Command InsertCommandNodeAfter(TreeNode<Command> commandNode, Command commandAfter)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(commandNode.value), "Source Command should not exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(commandAfter), "Destination Command should exist on script");

            var nodeAfter = Commands.GetNodeFromValue(commandAfter);
            var indexAfter = nodeAfter.parent.IndexOf(commandAfter);

            foreach (var node in commandNode.GetAllNodes())
                CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(node.value);

            var nodeToAddCommand = nodeAfter.parent;
            nodeToAddCommand.Join(commandNode);
            Commands.MoveAfter(commandNode.value, commandAfter);

            CommandInsertedInScript?.Invoke(this, nodeAfter.parent.value, commandNode.value, GetIndex(commandNode.value));

            m_IsDirty = true;
            DEBUG_CheckCommandGuidConsistency();

            return commandNode.value;
        }

        /// <summary>
        /// Inserts non existant command node before specified command with all its children.
        /// Calls CommandInserted event
        /// </summary>
        public Command InsertCommandNodeBefore(TreeNode<Command> commandNode, Command commandAfter)
        {
            Debug.Assert(!Commands.GetAllNodes().Select(n => n.value).Contains(commandNode.value), "Source Command should not exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(commandAfter), "Destination Command should exist on script");

            var nodeAfter = Commands.GetNodeFromValue(commandAfter);
            var indexAfter = nodeAfter.parent.IndexOf(commandAfter);

            foreach (var node in commandNode.GetAllNodes())
                CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(node.value);

            var nodeToAddCommand = nodeAfter.parent;
            nodeToAddCommand.Join(commandNode);
            Commands.MoveBefore(commandNode.value, commandAfter);

            CommandInsertedInScript?.Invoke(this, nodeAfter.parent.value, commandNode.value, GetIndex(commandNode.value));

            m_IsDirty = true;
            DEBUG_CheckCommandGuidConsistency();

            return commandNode.value;
        }

        /// <summary>
        /// Moves an existing command from script to other place.
        /// Calls Removed and Inserted command events.
        /// </summary>
        public void MoveCommandAfter(Command source, Command after)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(source), "Source Command should exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(after), "Destination Command should exist on script");

            var oldIndex = GetIndex(source);
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(after).parent.value;

            Commands.MoveAfter(source, after);

            CommandRemovedFromScript?.Invoke(this, sourceParentCommand, oldIndex);
            CommandInsertedInScript?.Invoke(this, destParentCommand, source, GetIndex(source));
            m_IsDirty = true;

            DEBUG_CheckCommandGuidConsistency();
        }

        /// <summary>
        /// Moves an existing command from script to other place.
        /// Calls Removed and Inserted command events.
        /// </summary>
        public void MoveCommandBefore(Command source, Command before)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(source), "Source Command should exist on script");
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(before), "Destination Command should exist on script");

            var oldIndex = GetIndex(source);
            var sourceParentCommand = Commands.GetNodeFromValue(source).parent.value;
            var destParentCommand = Commands.GetNodeFromValue(before).parent.value;

            Commands.MoveBefore(source, before);

            CommandRemovedFromScript?.Invoke(this, sourceParentCommand, oldIndex);
            CommandInsertedInScript?.Invoke(this, destParentCommand, source, GetIndex(source));
            m_IsDirty = true;

            DEBUG_CheckCommandGuidConsistency();
        }

        /// <summary>
        /// Removes node containing command. All child commands are also removed from script
        /// </summary>
        public void RemoveCommand(Command command)
        {
            Debug.Assert(Commands.GetAllNodes().Select(n => n.value).Contains(command), "Command should exist on script");

            var oldIndex = GetIndex(command);
            var commandNode = Commands.GetNodeFromValue(command);
            var parentCommand = commandNode.parent.value;

            Commands.Remove(commandNode);

            foreach (var node in commandNode.GetAllNodes(true))
                CommandGuidMap.RemoveGuidFromMap(node.value);

            CommandRemovedFromScript?.Invoke(this, parentCommand, oldIndex);
            m_IsDirty = true;

            DEBUG_CheckCommandGuidConsistency();
        }

        /// <summary>
        /// Clones command with child commands. Regenerates Guids to be unique.
        /// </summary>
        public TreeNode<Command> CloneCommandStub(Command command)
        {
            var node = Commands.GetNodeFromValue(command);
            var clone = (TreeNode<Command>)node.Clone();
            clone.CastAndRemoveNullsTree<IHaveGuid>().RegenerateGuids();
            return clone;
        }

        public bool HasRegisteredGuid(Guid guid)
        {
            return CommandGuidMap.Contains(guid);
        }

        public TreeNode<Command> GetCommandNode(Guid guid)
        {
            return Commands.GetAllNodes(false).FirstOrDefault(c => c.value.Guid == guid);
        }

        public int GetIndex(Command command)
        {
            var node = Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public object Clone()
        {
            return new Script((TreeNode<Command>)Commands.Clone())
            {
                Name = Name,
                m_IsDirty = true,
                Guid = Guid
            };
        }

        public override string ToString()
        {
            if (m_IsDirty)
                return Name + "*";
            else
                return Name;
        }

        public string Path
        {
            set
            {
                m_Path = value;
                m_Name = (Path == "") ? DefaultScriptName : Paths.GetName(Path);
                IsDirty = false;
            }
            get { return m_Path; }
        }

        private string m_Name;
        public override string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    IsDirty = true;
                }
            }
        }

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

        public bool Similar(object obj)
        {
            var s = obj as Script;
            if (s == null)
                return false;

            if (s.Name != Name)
                return false;

            var allCommands1 = s.Commands.GetAllNodes().Select(node => node.value);
            var allCommands2 = Commands.GetAllNodes().Select(node => node.value);

            return allCommands1.SequenceEqual(allCommands2, new SimilarEqualityComparer());
        }

        // Inheritence

        public Script(TreeNode<Command> commands) : base(commands)
        {
            Commands = commands;

            foreach (var node in Commands.GetAllNodes(false))
                CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(node.value);

            DEBUG_CheckCommandGuidConsistency();
        }

        public LightScript ToLightScript()
        {
            return new LightScript(Commands, Guid) { Name = Name };
        }

        public Script(LightScript lightScript)
        {
            Commands = lightScript.Commands;
            m_Name = lightScript.Name;
            Guid = lightScript.Guid;

            foreach (var node in Commands.GetAllNodes(false))
                CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(node.value);

            DEBUG_CheckCommandGuidConsistency();
        }

        public static Script FromLightScript(LightScript lightScript)
        {
            return new Script(lightScript) { Name = lightScript.Name, Guid = lightScript.Guid };
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

        /// <summary>
        /// Implemented explicitly so it has less visibility, since most systems should not regenerate guids by themself.
        /// As of this time, only scripts need to regenerate guids for commands (2018.08.15)
        /// </summary>
        void IHaveGuid.RegenerateGuid()
        {
            Guid = Guid.NewGuid();

            CommandGuidMap.Clear();
            var allCommandGuids = Commands.CastAndRemoveNullsTree<IHaveGuid>();
            foreach (var g in allCommandGuids)
            {
                g.RegenerateGuid();
                CommandGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(g);
            }
        }

        [Conditional("DEBUG")]
        private void DEBUG_CheckCommandGuidConsistency()
        {
            var allCommands = Commands.GetAllNodes(false).Select(node => node.value);
            foreach (var c in allCommands)
            {
                if (!CommandGuidMap.Contains(c.Guid))
                    Logger.Log(LogType.Error, "Command is not registered to guid map: " + c.ToString());
            }
        }
    }
}
