using System;
using System.Collections.Generic;
using Robot.Scripts;
using RobotRuntime;

namespace Robot.Abstractions
{
    public interface IScriptManager
    {
        Script ActiveScript { get; set; }
        IList<Script> LoadedScripts { get; }

        event Action<Script, Script> ActiveScriptChanged;
        event Action<Script, Command, Command> CommandAddedToScript;
        event Action<Script, Command, Command, int> CommandInsertedInScript;
        event Action<Script, Command, Command> CommandModifiedOnScript;
        event Action<Script, Command, int> CommandRemovedFromScript;
        event Action<Script> ScriptLoaded;
        event Action<Script> ScriptModified;
        event Action ScriptPositioningChanged;
        event Action<int> ScriptRemoved;
        event Action<Script> ScriptSaved;

        IEnumerator<Script> GetEnumerator();
        Script GetScriptFromCommand(Command command);
        Script LoadScript(string path);
        void MoveCommandAfter(Command source, Command after, int scriptIndex, int destinationScriptIndex = -1);
        void MoveCommandBefore(Command source, Command before, int scriptIndex, int destinationScriptIndex = -1);
        void MoveScriptAfter(int index, int after);
        void MoveScriptBefore(int index, int before);
        Script NewScript(Script clone = null);
        void RemoveScript(Script script);
        void RemoveScript(int position);
        void SaveScript(Script script, string path);
    }
}