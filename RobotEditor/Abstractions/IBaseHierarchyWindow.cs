using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Robot.Abstractions;
using RobotEditor.Hierarchy;
using RobotRuntime.Recordings;

namespace RobotEditor.Abstractions
{
    public interface IBaseHierarchyWindow
    {
        ToolStrip ToolStrip { get; }
        event Action<IBaseHierarchyManager, object> OnSelectionChanged;


        // Virtual methods below
        bool ShouldCancelDrop(HierarchyNode targetNode, HierarchyNode sourceNode, ModelDropEventArgs e);

        void HandleModelsDrop(ModelDropEventArgs e, HierarchyNode targetNode, IEnumerable<HierarchyNode> sourceNodes);
        void HandleModelsSelection(ModelDropEventArgs e, HierarchyNode targetNode, IEnumerable<HierarchyNode> sourceNodes);

        void HandleCommandModelDropFromWithinWindow(ModelDropEventArgs e, HierarchyNode targetNode, Recording sourceRecording, HierarchyNode sourceNode);
        void HandleRecordingModelDropFromWithinWindow(ModelDropEventArgs e, HierarchyNode targetNode, HierarchyNode sourceNode);

        void HandleCommandModelDropFromExternalWindow(ModelDropEventArgs e, HierarchyNode targetNode, Recording sourceRecording, HierarchyNode sourceNode);
        void HandleRecordingModelDropFromExternalWindow(ModelDropEventArgs e, HierarchyNode targetNode, HierarchyNode sourceNode);

        /// <summary>
        /// Callback when model is dropped to another window.
        /// The receiver window will call this callback. It is usually used to remove commands from source window, 
        /// when same commands are added to the destination window.
        /// </summary>
        void DragAndDropAcceptedCallback(HierarchyNode node);
    }
}
