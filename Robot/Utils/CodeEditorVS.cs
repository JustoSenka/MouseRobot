using EnvDTE;
using Robot.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Process = System.Diagnostics.Process;

namespace Robot.Utils
{
    public class CodeEditorVS : ICodeEditor
    {
        private const string VS_DTE_ID_REGEX = @"(?i)(!VisualStudio\.DTE\.\d+\.\d+:)";

        private int m_ProcessID;

        public DTE GetDteFromProccessID(int processID)
        {
            DTE dte = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;
            IEnumerable<Tuple<string, IMoniker>> list = null;

            try
            {
                list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);
                //var dtes = list.Select(t => t.Item2).Cast<DTE>();

                var first = list.FirstOrDefault(t => t.Item1.Contains(m_ProcessID.ToString()));
                Debug.Assert(first != null);

                object runningObject;
                Marshal.ThrowExceptionForHR(rot.GetObject(first.Item2, out runningObject));

                dte = runningObject as DTE;
                Debug.Assert(dte != null);
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
            return dte;
        }

        private IEnumerable<Tuple<string, DTE>> GetDteListfromROT(IEnumerable<Tuple<string, IMoniker>> monikerList, IRunningObjectTable rot)
        {
            foreach (var t in monikerList)
            {
                object runningObject;
                rot.GetObject(t.Item2, out runningObject);
                var dte = runningObject as DTE;
                if (dte != null)
                    yield return new Tuple<string, DTE>(t.Item1, dte);
            }
        }

        private IEnumerable<Tuple<string, IMoniker>> GetMonikerListFromROT(out IBindCtx bindCtx, out IRunningObjectTable rot, out IEnumMoniker enumMonikers)
        {
            Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
            bindCtx.GetRunningObjectTable(out rot);
            rot.EnumRunning(out enumMonikers);

            IMoniker[] moniker = new IMoniker[1];
            IntPtr numberFetched = IntPtr.Zero;

            var list = new List<Tuple<string, IMoniker>>();

            while (enumMonikers.Next(1, moniker, numberFetched) == 0)
            {
                IMoniker runningObjectMoniker = moniker[0];

                if (runningObjectMoniker == null)
                    continue;

                string name = "";
                try
                {
                    runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                }
                catch (UnauthorizedAccessException) { } // There is something in the ROT that we do not have access to.

                Debug.WriteLine("Name: " + name);

                if (Regex.IsMatch(name, VS_DTE_ID_REGEX))
                    list.Add(new Tuple<string, IMoniker>(name, runningObjectMoniker));
            }

            return list;
        }

        public bool FocusFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool StartEditor(string solutionPath)
        {
            var devenv = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe",
    @"C:\MouseRobotProject\MouseRobotProject_Solution.sln");

            m_ProcessID = devenv.Id;
            DTE dte = null;

            System.Threading.Thread.Sleep(4000);
            dte = GetDteFromProccessID(m_ProcessID);

            while (dte == null)
            {
                System.Threading.Thread.Sleep(50);
                dte = GetDteFromProccessID(m_ProcessID);
            }

            dte.ExecuteCommand("View.CommandWindow");
            dte.StatusBar.Text = "Hello World!";
            System.Threading.Thread.Sleep(2000);
            dte.ExecuteCommand("File.Exit");
            devenv.WaitForExit();
            Marshal.ReleaseComObject(dte);
            return true;
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
    }
}
