using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Process = System.Diagnostics.Process;

namespace Tests
{
    [TestClass]
    public class DteTests
    {
        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            var vs = new CodeEditorVS();

            var devenv = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe",
    @"C:\MouseRobotProject\MouseRobotProject_Solution.sln");

            DTE dte = null;


            System.Threading.Thread.Sleep(4000);
            dte = vs.GetDteFromProccessID(devenv.Id);


            dte.ExecuteCommand("View.CommandWindow");
            dte.StatusBar.Text = "Hello World!";
            System.Threading.Thread.Sleep(2000);
            dte.ExecuteCommand("File.Exit");
            devenv.WaitForExit();
            Marshal.ReleaseComObject(dte);
        }

    }
    /*
    public class AutomateVS
    {
        private const string VS_DTE_ID_REGEX = @"(?i)(!VisualStudio\.DTE\.\d+\.\d+:)";

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static DTE GetDTE(int processId)
        {
            string progId = "!VisualStudio.DTE.15.0:" + processId.ToString();
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                GetListOfMonikerFromROT(bindCtx, rot, enumMonikers);
            }
            finally
            {
                if (enumMonikers != null)
                    Marshal.ReleaseComObject(enumMonikers);

                if (rot != null)
                    Marshal.ReleaseComObject(rot);

                if (bindCtx != null)
                    Marshal.ReleaseComObject(bindCtx);
            }

            return (DTE)runningObject;
        }

        private static IEnumerable<IMoniker> GetListOfMonikerFromROT(IBindCtx bindCtx, IRunningObjectTable rot, IEnumMoniker enumMonikers)
        {
            IMoniker[] moniker = new IMoniker[1];
            IntPtr numberFetched = IntPtr.Zero;
            while (enumMonikers.Next(1, moniker, numberFetched) == 0)
            {
                IMoniker runningObjectMoniker = moniker[0];

                string name = null;

                if (runningObjectMoniker == null)
                    continue;

                try
                {
                    runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                }
                catch (UnauthorizedAccessException) { } // There is something in the ROT that we do not have access to.

                Debug.WriteLine("Name: " + name);

                if (!string.IsNullOrEmpty(name) && Regex.IsMatch(name, VS_DTE_ID_REGEX);
                {
                    yield return runningObjectMoniker;

                    Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                    Debug.WriteLine("Obj found");
                    //break;
                }
            }

            return runningObject;*/

            /*IMoniker[] moniker = new IMoniker[1];
            IntPtr numberFetched = IntPtr.Zero;
            while (enumMonikers.Next(1, moniker, numberFetched) == 0)
            {
                IMoniker runningObjectMoniker = moniker[0];

                string name = null;

                if (runningObjectMoniker == null)
                    continue;

                try
                {
                    runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                }
                catch (UnauthorizedAccessException) { } // There is something in the ROT that we do not have access to.

                Debug.WriteLine("Name: " + name);

                if (!string.IsNullOrEmpty(name) && string.Equals(name, progId, StringComparison.Ordinal))
                {
                    Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                    Debug.WriteLine("Obj found");
                    //break;
                }
            }

            return runningObject;*/
       // }
       /*
        public static void Main(string[] args)
        {
            var devenv = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe",
                @"C:\MouseRobotProject\MouseRobotProject_Solution.sln");

            DTE dte = null;


            System.Threading.Thread.Sleep(4000);
            dte = GetDTE(devenv.Id);


            
            do
            {
                System.Threading.Thread.Sleep(2000);
                dte = GetDTE(devenv.Id);
            }
            while (dte == null);

            dte.ExecuteCommand("View.CommandWindow");
            dte.StatusBar.Text = "Hello World!";
            System.Threading.Thread.Sleep(2000);
            dte.ExecuteCommand("File.Exit");
            devenv.WaitForExit();
            Marshal.ReleaseComObject(dte);
        }
    }*/

}
