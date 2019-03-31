using Robot.Recordings;
using Robot.Tests;
using System;
using System.Windows.Forms;

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