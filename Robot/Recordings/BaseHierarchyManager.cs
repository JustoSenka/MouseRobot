using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Robot.Recordings
{
    public abstract class BaseHierarchyManager : IEnumerable<RobotRuntime.Recordings.Recording>, IHaveGuidMap
    {
        protected readonly IList<RobotRuntime.Recordings.Recording> m_LoadedScripts;
        public IList<RobotRuntime.Recordings.Recording> LoadedScripts { get { return m_LoadedScripts; } }

        public event Action<RobotRuntime.Recordings.Recording> ScriptAdded;
        public event Action<RobotRuntime.Recordings.Recording> ScriptModified;
        public event Action<int> ScriptRemoved;
        public event Action ScriptPositioningChanged;

        public event Action<RobotRuntime.Recordings.Recording, Command, Command> CommandAddedToScript;
        public event Action<RobotRuntime.Recordings.Recording, Command, Command, int> CommandInsertedInScript;
        public event Action<RobotRuntime.Recordings.Recording, Command, int> CommandRemovedFromScript;
        public event Action<RobotRuntime.Recordings.Recording, Command, Command> CommandModifiedOnScript;

        protected readonly HashSet<Guid> ScriptGuidMap = new HashSet<Guid>();

        private ICommandFactory CommandFactory;
        private IProfiler Profiler;
        private ILogger Logger;
        public BaseHierarchyManager(ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
        {
            this.CommandFactory = CommandFactory;
            this.Profiler = Profiler;
            this.Logger = Logger;

            CommandFactory.NewUserCommands += ReplaceCommandsInScriptsWithNewRecompiledOnes;

            m_LoadedScripts = new List<RobotRuntime.Recordings.Recording>();
        }

        protected void SubscribeToScriptEvents(RobotRuntime.Recordings.Recording s)
        {
            s.CommandAddedToScript += InvokeCommandAddedToScript;
            s.CommandInsertedInScript += InvokeCommandInsertedInScript;
            s.CommandRemovedFromScript += InvokeCommandRemovedFromScript;
            s.CommandModifiedOnScript += InvokeCommandModifiedOnScript;
        }

        protected void UnsubscribeToScriptEvents(RobotRuntime.Recordings.Recording s)
        {
            s.CommandAddedToScript -= InvokeCommandAddedToScript;
            s.CommandInsertedInScript -= InvokeCommandInsertedInScript;
            s.CommandRemovedFromScript -= InvokeCommandRemovedFromScript;
            s.CommandModifiedOnScript -= InvokeCommandModifiedOnScript;
        }

        private void InvokeCommandAddedToScript(RobotRuntime.Recordings.Recording script, Command parentCommand, Command command)
        {
            CommandAddedToScript?.Invoke(script, parentCommand, command);
        }

        private void InvokeCommandInsertedInScript(RobotRuntime.Recordings.Recording script, Command parentCommand, Command command, int index)
        {
            CommandInsertedInScript?.Invoke(script, parentCommand, command, index);
        }

        private void InvokeCommandRemovedFromScript(RobotRuntime.Recordings.Recording script, Command parentCommand, int index)
        {
            CommandRemovedFromScript?.Invoke(script, parentCommand, index);
        }

        private void InvokeCommandModifiedOnScript(RobotRuntime.Recordings.Recording script, Command oldCommand, Command newCommand)
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

        public virtual Recording NewScript(RobotRuntime.Recordings.Recording clone = null)
        {
            RobotRuntime.Recordings.Recording script;

            if (clone == null)
                script = new RobotRuntime.Recordings.Recording();
            else
            {
                script = (RobotRuntime.Recordings.Recording)clone.Clone();
                ((IHaveGuid)script).RegenerateGuid();
            }

            ScriptGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(script);
            m_LoadedScripts.Add(script);

            SubscribeToScriptEvents(script);
            script.IsDirty = true;

            ScriptAdded?.Invoke(script);
            return script;
        }

        public virtual void RemoveScript(RobotRuntime.Recordings.Recording script)
        {
            var position = m_LoadedScripts.IndexOf(script);

            ScriptGuidMap.RemoveGuidFromMap(script);
            m_LoadedScripts.Remove(script);
            UnsubscribeToScriptEvents(script);

            ScriptRemoved?.Invoke(position);
        }

        public virtual void RemoveScript(int position)
        {
            UnsubscribeToScriptEvents(m_LoadedScripts[position]);

            ScriptGuidMap.RemoveGuidFromMap(m_LoadedScripts[position]);
            m_LoadedScripts.RemoveAt(position);

            ScriptRemoved?.Invoke(position);
        }

        public virtual Recording AddScript(RobotRuntime.Recordings.Recording script, bool removeScriptWithSamePath = false)
        {
            if (script == null)
                return null;

            // If script was already loaded, reload it to last saved state
            var oldScript = m_LoadedScripts.FirstOrDefault(s => s.Path.Equals(script.Path));
            if (oldScript != default(RobotRuntime.Recordings.Recording) && removeScriptWithSamePath)
            {
                // Reload Script
                var index = m_LoadedScripts.IndexOf(oldScript);
                UnsubscribeToScriptEvents(oldScript);

                ScriptGuidMap.RemoveGuidFromMap(m_LoadedScripts[index]);
                ScriptGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(script);

                m_LoadedScripts[index] = script;
                ScriptModified?.Invoke(script);
            }
            else
            {
                // Load New Script
                ScriptGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(script);
                m_LoadedScripts.Add(script);
                ScriptAdded?.Invoke(script);
            }

            SubscribeToScriptEvents(script);
            return script;
        }

        /// <summary>
        /// Moves existing command from script to different place and/or between scripts.
        /// Also moves all child commands altogether.
        /// </summary>
        public void MoveCommandAfter(Command source, Command after, int scriptIndex, int destinationScriptIndex = -1) // script indices could be removed
        {
            var script = m_LoadedScripts[scriptIndex];

            if (scriptIndex == destinationScriptIndex || destinationScriptIndex == -1) // Same script
                script.MoveCommandAfter(source, after);

            else // Move between two different scripts
            {
                var destScript = m_LoadedScripts[destinationScriptIndex];
                var sourceNode = script.Commands.GetNodeFromValue(source);

                if (Logger.AssertIf(sourceNode == null,
                    "Cannot find node in script '" + destScript.Name + "' for command: " + source))
                    return;

                script.RemoveCommand(source);
                destScript.InsertCommandNodeAfter(sourceNode, after);

                destScript.IsDirty = true;
            }

            script.IsDirty = true;
        }

        /// <summary>
        /// Moves existing command from script to different place and/or between scripts.
        /// Also moves all child commands altogether.
        /// </summary>
        public void MoveCommandBefore(Command source, Command before, int scriptIndex, int destinationScriptIndex = -1) // script indices could be removed
        {
            var script = m_LoadedScripts[scriptIndex];

            if (scriptIndex == destinationScriptIndex || destinationScriptIndex == -1) // Same script
                script.MoveCommandBefore(source, before);

            else // Move between two different scripts
            {
                var destScript = m_LoadedScripts[destinationScriptIndex];
                var sourceNode = m_LoadedScripts[scriptIndex].Commands.GetNodeFromValue(source);

                if (Logger.AssertIf(sourceNode == null,
                    "Cannot find node in script '" + destScript.Name + "' for command: " + source))
                    return;

                script.RemoveCommand(source);
                destScript.InsertCommandNodeBefore(sourceNode, before);

                destScript.IsDirty = true;
            }

            script.IsDirty = true;
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

        public Recording GetScriptFromCommand(Command command)
        {
            return LoadedScripts.FirstOrDefault((s) => s.Commands.GetAllNodes(false).Select(n => n.value).Contains(command));
        }

        public Recording GetScriptFromCommandGuid(Guid guid)
        {
            return LoadedScripts.FirstOrDefault((s) => s.Commands.GetAllNodes(false).Select(n => n.value.Guid).Contains(guid));
        }

        public int GetCommandIndex(Command command)
        {
            var script = GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public int GetScriptIndex(RobotRuntime.Recordings.Recording script)
        {
            return LoadedScripts.IndexOf(script);
        }

        public bool HasRegisteredGuid(Guid guid)
        {
            return ScriptGuidMap.Contains(guid);
        }

        public IEnumerator<RobotRuntime.Recordings.Recording> GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }

        [Conditional("DEBUG")]
        private void CheckCommandGuidConsistency()
        {
            foreach (var s in m_LoadedScripts)
            {
                if (!ScriptGuidMap.Contains(s.Guid))
                    Logger.Logi(LogType.Error, "Script is not registered to guid map: " + s.ToString());
            }
        }
    }
}
