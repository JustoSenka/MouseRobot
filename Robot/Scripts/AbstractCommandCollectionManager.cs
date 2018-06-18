using Robot.Abstractions;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Robot
{
    public class AbstractCommandCollectionManager : IEnumerable<CommandCollection>
    {
        private readonly IList<CommandCollection> m_LoadedScripts;
        public IList<CommandCollection> LoadedScripts { get { return m_LoadedScripts; } }

        public event Action ScriptPositioningChanged;

        public event Action<CommandCollection, Command, Command> CommandAddedToCollection;
        public event Action<CommandCollection, Command, Command, int> CommandInsertedInCollection;
        public event Action<CommandCollection, Command, int> CommandRemovedFromCollection;
        public event Action<CommandCollection, Command, Command> CommandModifiedOnCollection;

        private IAssetManager AssetManager;
        private ICommandFactory CommandFactory;
        private IProfiler Profiler;
        public AbstractCommandCollectionManager(IAssetManager AssetManager, ICommandFactory CommandFactory, IProfiler Profiler)
        {
            this.AssetManager = AssetManager;
            this.CommandFactory = CommandFactory;
            this.Profiler = Profiler;

            CommandFactory.NewUserCommands += ReplaceCommandsInScriptsWithNewRecompiledOnes;

            m_LoadedScripts = new List<CommandCollection>();
        }

        public void InvokeCommandAddedToScript(CommandCollection script, Command parentCommand, Command command)
        {
            CommandAddedToCollection?.Invoke(script, parentCommand, command);
        }

        public void InvokeCommandInsertedInScript(CommandCollection script, Command parentCommand, Command command, int index)
        {
            CommandInsertedInCollection?.Invoke(script, parentCommand, command, index);
        }

        public void InvokeCommandRemovedFromScript(CommandCollection script, Command parentCommand, int index)
        {
            CommandRemovedFromCollection?.Invoke(script, parentCommand, index);
        }

        public void InvokeCommandModifiedOnScript(CommandCollection script, Command oldCommand, Command newCommand)
        {
            CommandModifiedOnCollection?.Invoke(script, oldCommand, newCommand);
        }

        private void ReplaceCommandsInScriptsWithNewRecompiledOnes()
        {
            Profiler.Start("ScriptManager_ReplaceOldCommandInstances");

            foreach (var script in m_LoadedScripts)
            {
                foreach (var node in script.Commands.GetAllNodes().ToArray())
                {
                    var command = node.value;
                    if (command == null || CommandFactory.IsNative(command))
                        continue;

                    script.ReplaceCommand(node.value, CommandFactory.Create(node.value.Name, node.value));
                }
            }

            Profiler.Stop("ScriptManager_ReplaceOldCommandInstances");
        }
        
        public void MoveCommandAfter(Command source, Command after, int scriptIndex, int destinationScriptIndex = -1) // script indices could be removed
        {
            if (scriptIndex == destinationScriptIndex || destinationScriptIndex == -1) // Same script
            {
                var script = m_LoadedScripts[scriptIndex];
                script.MoveCommandAfter(source, after);
            }
            else // Move between two different scripts
            {
                var destScript = m_LoadedScripts[destinationScriptIndex];
                var destParentNode = m_LoadedScripts[destinationScriptIndex].Commands.GetNodeFromValue(after).parent;
                var sourceNode = m_LoadedScripts[scriptIndex].Commands.GetNodeFromValue(source);

                m_LoadedScripts[scriptIndex].RemoveCommand(source);

                destParentNode.Join(sourceNode);
                destScript.Commands.MoveAfter(source, after);

                CommandInsertedInCollection?.Invoke(destScript, destParentNode.value, source, source.GetIndex(this));
                
                destScript.IsDirty = true;
            }

            m_LoadedScripts[scriptIndex].IsDirty = true;
        }

        public void MoveCommandBefore(Command source, Command before, int scriptIndex, int destinationScriptIndex = -1) // script indices could be removed
        {
            if (scriptIndex == destinationScriptIndex || destinationScriptIndex == -1) // Same script
            {
                var script = m_LoadedScripts[scriptIndex];
                script.MoveCommandBefore(source, before);
            }
            else // Move between two different scripts
            {
                var destScript = m_LoadedScripts[destinationScriptIndex];
                var destParentNode = m_LoadedScripts[destinationScriptIndex].Commands.GetNodeFromValue(before).parent;
                var sourceNode = m_LoadedScripts[scriptIndex].Commands.GetNodeFromValue(source);

                m_LoadedScripts[scriptIndex].RemoveCommand(source);

                destParentNode.Join(sourceNode);
                destScript.Commands.MoveBefore(source, before);

                CommandInsertedInCollection?.Invoke(destScript, destParentNode.value, source, source.GetIndex(this));

                destScript.IsDirty = true;
            }

            m_LoadedScripts[scriptIndex].IsDirty = true;
        }

        public void MoveScriptAfter(int index, int after)
        {
            m_LoadedScripts.MoveAfter(index, after);
            ScriptPositioningChanged?.Invoke();
        }

        public void MoveScriptBefore(int index, int before)
        {
            m_LoadedScripts.MoveBefore(index, before);
            ScriptPositioningChanged?.Invoke();
        }

        public CommandCollection GetScriptFromCommand(Command command)
        {
            return m_LoadedScripts.FirstOrDefault((s) => s.Commands.GetAllNodes().Select(n => n.value).Contains(command));
        }

        public IEnumerator<CommandCollection> GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }
    }
}
