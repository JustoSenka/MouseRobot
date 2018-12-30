using EnvDTE;
using Robot.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private string m_VsProgID = "";
        private int m_VsProcessID;

        private ISolutionManager SolutionManager;
        public CodeEditorVS(ISolutionManager SolutionManager)
        {
            this.SolutionManager = SolutionManager;
        }

        public bool FocusFile(string filePath)
        {
            if (m_VsProgID.IsEmpty()) // VS ProgID is unknown. Lets check if user opened VS himself and try to find exact instance.
                m_VsProgID = GetProgIdFromSolutionName(SolutionManager.CSharpSolutionName);

            if (m_VsProgID.IsEmpty()) // VS ProgID is still unknown, lets open VS on our own
                StartEditor(SolutionManager.CSharpSolutionPath);

            if (m_VsProgID.IsEmpty()) // Opening failed
                return false;

            var dte = GetDteFromProgID(m_VsProgID);
            dte.StatusBar.Text = "Focus File!";

            dte.ItemOperations.OpenFile(filePath);

            return true;
        }

        public bool StartEditor(string solutionPath)
        {
            m_VsProcessID = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe",
                Path.Combine(Environment.CurrentDirectory, solutionPath)).Id;

            WaitForVsToAppearInRotAndGetProgID();

            WaitForVsToBeInitialized();

            return true;
        }

        private void WaitForVsToAppearInRotAndGetProgID()
        {
            var numOfTries = 100;

            while (m_VsProgID.IsEmpty() && numOfTries > 0)
            {
                numOfTries--;

                System.Threading.Thread.Sleep(60);
                m_VsProgID = GetProgIDFromProccessID(m_VsProcessID);
            }
        }

        private bool WaitForVsToBeInitialized()
        {
            var dte = GetDteFromProgID(m_VsProgID);

            var numOfTries = 10;

            var vsInitialized = false;
            while (!vsInitialized && numOfTries > 0)
            {
                numOfTries--;

                try
                {
                    System.Threading.Thread.Sleep(60); 
                    dte.StatusBar.Text = "Launched from Mouse Robot!";
                    vsInitialized = true;
                }
                catch
                {
                    vsInitialized = false;
                }
            }

            return vsInitialized;
        }

        // ----------------

        private string GetProgIdFromSolutionName(string solutionName)
        {
            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                var list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);
                var dteList = GetDteListFromROT(list, rot);

                var tuple = dteList.FirstOrDefault(t =>
                    Path.GetFileName(t.Item2.Solution.FullName).Equals(solutionName, StringComparison.InvariantCultureIgnoreCase));

                return tuple.Item1;
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
        }

        private DTE GetDteFromProgID2(string progId)
        {
            var visualStudioType = Type.GetTypeFromProgID(progId);
            return Activator.CreateInstance(visualStudioType) as DTE;
        }

        private DTE GetDteFromProgID(string progId)
        {
            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                var list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);

                var tuple = list.FirstOrDefault(t =>
                    t.Item1.Equals(progId, StringComparison.InvariantCultureIgnoreCase));

                Marshal.ThrowExceptionForHR(rot.GetObject(tuple.Item2, out object runningObject));
                return runningObject as DTE;
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
        }

        private string GetProgIDFromProccessID(int processID)
        {
            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                var list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);

                var first = list.FirstOrDefault(t => t.Item1.Contains(processID.ToString()));
                return first.Item1;
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
        }

        private IEnumerable<(string, DTE)> GetDteListFromROT(IEnumerable<(string, IMoniker)> monikerList, IRunningObjectTable rot)
        {
            foreach (var t in monikerList)
            {
                Marshal.ThrowExceptionForHR(rot.GetObject(t.Item2, out object runningObject));
                if (runningObject is DTE dte)
                    yield return (t.Item1, dte);
                else
                    Marshal.ReleaseComObject(runningObject);
            }
        }

        private IEnumerable<(string, IMoniker)> GetMonikerListFromROT(out IBindCtx bindCtx, out IRunningObjectTable rot, out IEnumMoniker enumMonikers)
        {
            Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
            bindCtx.GetRunningObjectTable(out rot);
            rot.EnumRunning(out enumMonikers);

            IMoniker[] moniker = new IMoniker[1];
            IntPtr numberFetched = IntPtr.Zero;

            var list = new List<(string, IMoniker)>();

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
                    list.Add((name, runningObjectMoniker));
            }

            return list;
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
    }
}
