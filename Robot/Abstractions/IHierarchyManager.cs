using System;
using System.Collections.Generic;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot.Abstractions
{
    public interface IHierarchyManager
    {
        Recording ActiveScript { get; set; }
        IList<Recording> LoadedScripts { get; }

        event Action<Recording, Recording> ActiveScriptChanged;
        event Action<Recording, Command, Command> CommandAddedToScript;
        event Action<Recording, Command, Command, int> CommandInsertedInScript;
        event Action<Recording, Command, Command> CommandModifiedOnScript;
        event Action<Recording, Command, int> CommandRemovedFromScript;
        event Action<Recording> ScriptAdded;
        event Action<Recording> ScriptModified;
        event Action ScriptPositioningChanged;
        event Action<int> ScriptRemoved;
        event Action<Recording> ScriptSaved;

        IEnumerator<Recording> GetEnumerator();
        Recording GetScriptFromCommand(Command command);
        Recording GetScriptFromCommandGuid(Guid guid);
        Recording LoadScript(string path);
        void MoveCommandAfter(Command source, Command after, int scriptIndex, int destinationScriptIndex = -1);
        void MoveCommandBefore(Command source, Command before, int scriptIndex, int destinationScriptIndex = -1);
        void MoveScriptAfter(int index, int after);
        void MoveScriptBefore(int index, int before);
        Recording NewScript(Recording clone = null);
        void RemoveScript(Recording script);
        void RemoveScript(int position);
        void SaveScript(Recording script, string path);
    }
}