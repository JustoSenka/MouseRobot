using System;
using System.Windows.Forms;
using Robot.Scripts;
using RobotRuntime.Tests;

namespace RobotEditor.Abstractions
{
    public interface ITestFixtureWindow
    {
        ToolStrip ToolStrip { get; }

        event Action<BaseScriptManager, object> OnSelectionChanged;

        void DisplayTestFixture(TestFixture fixture);
        void SaveFixtureWithDialog(bool updateUI = true);
        void SaveTestFixture();
    }
}