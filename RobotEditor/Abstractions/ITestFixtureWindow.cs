using System;
using System.Windows.Forms;
using Robot.Recordings;
using RobotRuntime.Tests;

namespace RobotEditor.Abstractions
{
    public interface ITestFixtureWindow
    {
        ToolStrip ToolStrip { get; }

        event Action<BaseHierarchyManager, object> OnSelectionChanged;

        void DisplayTestFixture(TestFixture fixture);
        void SaveFixtureWithDialog(bool updateUI = true);
        void SaveTestFixture();
    }
}