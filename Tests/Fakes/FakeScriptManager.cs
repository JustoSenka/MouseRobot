using Robot.Abstractions;
using System;
using System.Collections.Generic;
using Robot.Scripts;
using RobotRuntime;

namespace Tests.Fakes
{
#pragma warning disable 0169
    class FakeScriptManager : IScriptManager
    {
        public Script ActiveScript
        {
            get
            {
                return null;
            }

            set
            {
                
            }
        }

        public IReadOnlyList<Script> LoadedScripts
        {
            get
            {
                return null;
            }
        }

        public event Action<Script, Script> ActiveScriptChanged;
        public event Action<Script, Command, Command> CommandAddedToScript;
        public event Action<Script, Command, Command, int> CommandInsertedInScript;
        public event Action<Script, Command, Command> CommandModifiedOnScript;
        public event Action<Script, Command, int> CommandRemovedFromScript;
        public event Action<Script> ScriptLoaded;
        public event Action<Script> ScriptModified;
        public event Action ScriptPositioningChanged;
        public event Action<int> ScriptRemoved;
        public event Action<Script> ScriptSaved;
#pragma warning restore 0169

        public IEnumerator<Script> GetEnumerator()
        {
            yield break;
        }

        public Script GetScriptFromCommand(Command command)
        {
            return null;
        }

        public void InvokeCommandAddedToScript(Script script, Command parentCommand, Command command)
        {
        }

        public void InvokeCommandInsertedInScript(Script script, Command parentCommand, Command command, int index)
        {
        }

        public void InvokeCommandModifiedOnScript(Script script, Command oldCommand, Command newCommand)
        {
        }

        public void InvokeCommandRemovedFromScript(Script script, Command parentCommand, int index)
        {
        }

        public Script LoadScript(string path)
        {
            return null;
        }

        public void MoveCommandAfter(Command source, Command after, int scriptIndex, int destinationScriptIndex = -1)
        {
        }

        public void MoveCommandBefore(Command source, Command before, int scriptIndex, int destinationScriptIndex = -1)
        {
        }

        public void MoveScriptAfter(int index, int after)
        {
        }

        public void MoveScriptBefore(int index, int before)
        {
        }

        public Script NewScript(Script clone = null)
        {
            return null;
        }

        public void RemoveScript(int position)
        {
        }

        public void RemoveScript(Script script)
        {
        }

        public void SaveScript(Script script, string path)
        {
        }
    }
}
