using System;
using System.Collections.Generic;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot.Abstractions
{
    public interface IHierarchyManager
    {
        Recording ActiveScript { get; set; }
        IList<RobotRuntime.Recordings.Recording> LoadedScripts { get; }

        event Action<RobotRuntime.Recordings.Recording, RobotRuntime.Recordings.Recording> ActiveScriptChanged;
        event Action<RobotRuntime.Recordings.Recording, Command, Command> CommandAddedToScript;
        event Action<RobotRuntime.Recordings.Recording, Command, Command, int> CommandInsertedInScript;
        event Action<RobotRuntime.Recordings.Recording, Command, Command> CommandModifiedOnScript;
        event Action<RobotRuntime.Recordings.Recording, Command, int> CommandRemovedFromScript;
        event Action<RobotRuntime.Recordings.Recording> ScriptAdded;
        event Action<RobotRuntime.Recordings.Recording> ScriptModified;
        event Action ScriptPositioningChanged;
        event Action<int> ScriptRemoved;
        event Action<RobotRuntime.Recordings.Recording> ScriptSaved;

        IEnumerator<RobotRuntime.Recordings.Recording> GetEnumerator();
        Recording GetScriptFromCommand(Command command);
        Recording GetScriptFromCommandGuid(Guid guid);
        Recording LoadScript(string path);
        void MoveCommandAfter(Command source, Command after, int scriptIndex, int destinationScriptIndex = -1);
        void MoveCommandBefore(Command source, Command before, int scriptIndex, int destinationScriptIndex = -1);
        void MoveScriptAfter(int index, int after);
        void MoveScriptBefore(int index, int before);
        Recording NewScript(RobotRuntime.Recordings.Recording clone = null);
        void RemoveScript(RobotRuntime.Recordings.Recording script);
        void RemoveScript(int position);
        void SaveScript(RobotRuntime.Recordings.Recording script, string path);
    }
}