using EnvDTE;
using Robot.Abstractions;
using RobotRuntime;
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
        private object VsProgIDLcok = new object();

        private ISolutionManager SolutionManager;
        public CodeEditorVS(ISolutionManager SolutionManager)
        {
            this.SolutionManager = SolutionManager;
        }

        public bool FocusFile(string filePath)
        {
            lock (VsProgIDLcok)
            {
                if (m_VsProgID.IsEmpty()) // VS ProgID is unknown. Lets check if user opened VS himself and try to find exact instance.
                    m_VsProgID = GetProgIdFromSolutionName(SolutionManager.CSharpSolutionName);

                if (m_VsProgID.IsEmpty()) // VS ProgID is still unknown, lets open VS on our own
                    StartEditorInternal(SolutionManager.CSharpSolutionPath);

                if (m_VsProgID.IsEmpty()) // Opening failed
                    return false;

                var dte = GetDteFromProgID(m_VsProgID);
                var processId = GetProcessIdFromProgID(m_VsProgID);
                if (dte == null && processId != 0 && !IsProcessRunning(processId))
                {
                    // DTE is null but processID was not empty, that means we have already interacted with VS before
                    // This can happen if user closes VS himself, lets check if he reopened it
                    m_VsProgID = GetProgIdFromSolutionName(SolutionManager.CSharpSolutionName);

                    if (m_VsProgID.IsEmpty()) // If he didd't reopened it, open one ourself
                        StartEditorInternal(SolutionManager.CSharpSolutionPath);

                    dte = GetDteFromProgID(m_VsProgID);
                }

                // TODO: dte can still be null here, if VS process fails to close itself, or refuses to communicate, should I open another VS?

                try
                {
                    dte.ItemOperations.OpenFile(Path.Combine(Environment.CurrentDirectory, filePath));
                }
                catch (Exception e)
                {
                    // NOTE: dte can still be null here, if process is up and running, but we failed to find correct progID. Or COM exception was thrown
                    Logger.Log(LogType.Error, "Cannot focus file in Code Editor (VS): " + filePath, e.Message);
                    return false;
                }
            }

            return true;
        }

        public bool StartEditor(string solutionPath)
        {
            lock (VsProgIDLcok)
            {
                return StartEditorInternal(solutionPath);
            }
        }

        private bool StartEditorInternal(string solutionPath)
        {
            m_VsProgID = "";

            /*m_VsProcessID = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe",
                Path.Combine(Environment.CurrentDirectory, solutionPath)).Id;*/

            var processId = Process.Start(Path.Combine(Environment.CurrentDirectory, solutionPath)).Id;

            if (!WaitForVsToAppearInRotAndGetProgID(processId))
            {
                Logger.Log(LogType.Error, "Cannot get ProgID from visual studio process");
                return false;
            }

            if (!WaitForVsToBeInitialized(m_VsProgID))
            {
                Logger.Log(LogType.Error, "VS DTE rejects calls with Exception from HRESULT: 0x80010001 (RPC_E_CALL_REJECTED)");
                return false;
            }

            return true;
        }

        private bool WaitForVsToAppearInRotAndGetProgID(int processId)
        {
            var numOfTries = 100;

            while (m_VsProgID.IsEmpty() && numOfTries > 0)
            {
                numOfTries--;

                System.Threading.Thread.Sleep(60);
                m_VsProgID = GetProgIDFromProccessID(processId);
            }

            return !m_VsProgID.IsEmpty();
        }

        private bool WaitForVsToBeInitialized(string progID)
        {
            var dte = GetDteFromProgID(progID);
            if (dte == null)
            {
                Logger.Log(LogType.Error, "Could not find Visual Studio instance from progID: " + progID);
                return false;
            }

            var numOfTries = 100;

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
            var obj = ExecuteActionInTryFinallyWithComObjectCleanup((bindCtx, rot, enumMonikers) =>
            {
                var list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);
                var dteList = GetDteListFromROT(list, rot);

                // dteList.FirstOrDefault(t =>
                // Path.GetFileName(t.Item2.Solution.FullName).Equals(solutionName, StringComparison.InvariantCultureIgnoreCase));

                var tuple = FirstOrDefaultInTryCatch(dteList, (dte) =>
                {
                    return Path.GetFileNameWithoutExtension(dte.Solution.FullName).Equals(solutionName, StringComparison.InvariantCultureIgnoreCase);
                });

                foreach (var dte in dteList)
                    Marshal.ReleaseComObject(dte.Item2);

                return tuple.Item1;
            });

            return obj as string;
        }

        private static (string, DTE) FirstOrDefaultInTryCatch(IEnumerable<(string, DTE)> dteList, Func<DTE, bool> predicate)
        {
            foreach (var tuple in dteList)
            {
                try
                {
                    if (predicate(tuple.Item2))
                        return tuple;
                }
                catch { } // DTE might refuse to talk if VS is busy, debuggin or has a popup dielog open
            }

            return default;
        }

        private DTE GetDteFromProgID(string progId)
        {
            var obj = ExecuteActionInTryFinallyWithComObjectCleanup((bindCtx, rot, enumMonikers) =>
            {
                var list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);

                var tuple = list.FirstOrDefault(t => t.Item1.Equals(progId, StringComparison.InvariantCultureIgnoreCase));

                if (tuple.Item2 == null)
                    return null;

                Marshal.ThrowExceptionForHR(rot.GetObject(tuple.Item2, out object runningObject));
                return runningObject;
            });

            return obj as DTE;
        }

        private string GetProgIDFromProccessID(int processID)
        {
            var obj = ExecuteActionInTryFinallyWithComObjectCleanup((bindCtx, rot, enumMonikers) =>
            {
                var list = GetMonikerListFromROT(out bindCtx, out rot, out enumMonikers);

                var first = list.FirstOrDefault(t => t.Item1.Contains(processID.ToString()));
                return first.Item1;
            });

            return obj as string;
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

        private object ExecuteActionInTryFinallyWithComObjectCleanup(Func<IBindCtx, IRunningObjectTable, IEnumMoniker, object> func)
        {
            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                return func(bindCtx, rot, enumMonikers);
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

        public static bool IsProcessRunning(int processID)
        {
            try
            {
                Process.GetProcessById(processID);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static int GetProcessIdFromProgID(string progID)
        {
            try
            {
                return int.Parse(progID.Split(':').Last().Trim());
            }
            catch
            {
                Logger.Log(LogType.Error, "Cannot parse process ID from prog ID: " + progID);
                return 0;
            }
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
    }
}
