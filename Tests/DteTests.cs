using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Utils;
using System.Runtime.InteropServices;
using Process = System.Diagnostics.Process;

namespace Tests
{
    [TestClass]
    public class DteTests
    {
        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
           /* var vs = new CodeEditorVS();

            var devenv = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe",
    @"C:\MouseRobotProject\MouseRobotProject_Solution.sln");

            System.Threading.Thread.Sleep(4000);
            var dte = vs.GetDteFromProccessID(devenv.Id);

            dte.ExecuteCommand("View.CommandWindow");
            dte.StatusBar.Text = "Hello World!";
            System.Threading.Thread.Sleep(2000);
            dte.ExecuteCommand("File.Exit");
            devenv.WaitForExit();
            Marshal.ReleaseComObject(dte);*/
        }
    }
}
