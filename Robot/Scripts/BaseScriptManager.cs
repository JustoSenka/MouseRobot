using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Robot.Scripts
{
    public abstract class BaseScriptManager : IEnumerable<Script>
    {
        protected readonly IList<Script> m_LoadedScripts;
        public IList<Script> LoadedScripts { get { return m_LoadedScripts; } }

        public event Action<Script> ScriptLoaded;
        public event Action<Script> ScriptModified;
        public event Action<int> ScriptRemoved;
        public event Action ScriptPositioningChanged;

        public event Action<Script, Command, Command> CommandAddedToScript;
        public event Action<Script, Command, Command, int> CommandInsertedInScript;
        public event Action<Script, Command, int> CommandRemovedFromScript;
        public event Action<Script, Command, Command> CommandModifiedOnScript;

        private ICommandFactory CommandFactory;
        private IProfiler Profiler;
        public BaseScriptManager(ICommandFactory CommandFactory, IProfiler Profiler)
        {
            this.CommandFactory = CommandFactory;
            this.Profiler = Profiler;

            CommandFactory.NewUserCommands += ReplaceCommandsInScriptsWithNewRecompiledOnes;

            m_LoadedScripts = new List<Script>();
        }

        protected void SubscribeToScriptEvents(Script s)
        {
            s.CommandAddedToScript += InvokeCommandAddedToScript;
            s.CommandInsertedInScript += InvokeCommandInsertedInScript;
            s.CommandRemovedFromScript += InvokeCommandRemovedFromScript;
            s.CommandModifiedOnScript += InvokeCommandModifiedOnScript;
        }

        protected void UnsubscribeToScriptEvents(Script s)
        {
            s.CommandAddedToScript -= InvokeCommandAddedToScript;
            s.CommandInsertedInScript -= InvokeCommandInsertedInScript;
            s.CommandRemovedFromScript -= InvokeCommandRemovedFromScript;
            s.CommandModifiedOnScript -= InvokeCommandModifiedOnScript;
        }

        private void InvokeCommandAddedToScript(Script script, Command parentCommand, Command command)
        {
            CommandAddedToScript?.Invoke(script, parentCommand, command);
        }

        private void InvokeCommandInsertedInScript(Script script, Command parentCommand, Command command, int index)
        {
            CommandInsertedInScript?.Invoke(script, parentCommand, command, index);
        }

        private void InvokeCommandRemovedFromScript(Script script, Command parentCommand, int index)
        {
            CommandRemovedFromScript?.Invoke(script, parentCommand, index);
        }

        private void InvokeCommandModifiedOnScript(Script script, Command oldCommand, Command newCommand)
        {
            CommandModifiedOnScript?.Invoke(script, oldCommand, newCommand);
        }

        private void ReplaceCommandsInScriptsWithNewRecompiledOnes()
        {
            Profiler.Start("BaseScriptManager_ReplaceOldCommandInstances");

            foreach (var script in LoadedScripts)
            {
                foreach (var node in script.Commands.GetAllNodes().ToArray())
                {
                    var command = node.value;
                    if (command == null || CommandFactory.IsNative(command))
                        continue;

                    script.ReplaceCommand(node.value, CommandFactory.Create(node.value.Name, node.value));
                }
            }

            Profiler.Stop("BaseScriptManager_ReplaceOldCommandInstances");
        }

        public virtual Script NewScript(Script clone = null)
        {
            Script script;

            if (clone == null)
                script = new Script();
            else
                script = (Script)clone.Clone();

            m_LoadedScripts.Add(script);
            SubscribeToScriptEvents(script);
            script.IsDirty = true;

            ScriptLoaded?.Invoke(script);
            return script;
        }

        public virtual void RemoveScript(Script script)
        {
            var position = m_LoadedScripts.IndexOf(script);

            m_LoadedScripts.Remove(script);
            UnsubscribeToScriptEvents(script);

            ScriptRemoved?.Invoke(position);
        }

        public virtual void RemoveScript(int position)
        {
            UnsubscribeToScriptEvents(m_LoadedScripts[position]);
            m_LoadedScripts.RemoveAt(position);

            ScriptRemoved?.Invoke(position);
        }

        public virtual Script AddScript(Script script, bool removeScriptWithSamePath = false)
        {
            // If script was already loaded, reload it to last saved state
            var oldScript = m_LoadedScripts.FirstOrDefault(s => s.Path.Equals(script.Path));
            if (oldScript != default(Script) && removeScriptWithSamePath)
            {
                // Reload Script
                var index = m_LoadedScripts.IndexOf(oldScript);
                UnsubscribeToScriptEvents(oldScript);

                m_LoadedScripts[index] = script;
                ScriptModified?.Invoke(script);
            }
            else
            {
                // Load New Script
                m_LoadedScripts.Add(script);
                ScriptLoaded?.Invoke(script);
            }

            SubscribeToScriptEvents(script);
            return script;
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

                CommandInsertedInScript?.Invoke(destScript, destParentNode.value, source, GetCommandIndex(source));

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

                CommandInsertedInScript?.Invoke(destScript, destParentNode.value, source, GetCommandIndex(source));

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

        public Script GetScriptFromCommand(Command command)
        {
            return LoadedScripts.FirstOrDefault((s) => s.Commands.GetAllNodes().Select(n => n.value).Contains(command));
        }

        public int GetCommandIndex(Command command)
        {
            var script = GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public IEnumerator<Script> GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }
    }
}
