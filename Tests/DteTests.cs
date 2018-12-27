﻿using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Tests
{
    [TestClass]
    public class DteTests
    {
        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {

            AutomateVS.Main(null);


        }

    }

    public class AutomateVS
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static DTE GetDTE(int processId)
        {
            string progId = "!VisualStudio.DTE.10.0:" + processId.ToString();
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        if (runningObjectMoniker != null)
                        {
                            runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    Console.WriteLine(name);
               
                    if (!string.IsNullOrEmpty(name) && string.Equals(name, progId, StringComparison.Ordinal))
                    {
                        Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return (DTE)runningObject;
        }

        public static void Main(string[] args)
        {
            var devenv = System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe");

            DTE dte = null;


            System.Threading.Thread.Sleep(2000);
            dte = GetDTE(devenv.Id);


            /*
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
            Marshal.ReleaseComObject(dte);*/
        }
    }

}
