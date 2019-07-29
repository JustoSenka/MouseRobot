using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RobotRuntime.Utils
{
    /// <summary>
    /// Starts invisible process, redirects console output automatically, creates no window as ran from command line
    /// </summary>
    public static class ProcessUtility
    {
        public static string StartFromCommandLine(string executablePath, string arguments, ProcessStartInfo processStartInfo, bool waitForExit = true)
        {
            processStartInfo.FileName = executablePath;
            processStartInfo.Arguments = arguments;
            return StartFromCommandLine(processStartInfo, waitForExit);
        }

        public static string StartFromCommandLine(string executablePath, ProcessStartInfo processStartInfo, bool waitForExit = true)
        {
            processStartInfo.FileName = executablePath;
            return StartFromCommandLine(processStartInfo, waitForExit);
        }

        public static string StartFromCommandLine(string executablePath, string arguments, bool waitForExit = true)
        {
            return StartFromCommandLine(new ProcessStartInfo(executablePath, arguments), waitForExit);
        }

        public static string StartFromCommandLine(string executablePath, bool waitForExit = true)
        {
            return StartFromCommandLine(new ProcessStartInfo(executablePath), waitForExit);
        }

        public static string StartFromCommandLine(ProcessStartInfo processStartInfo, bool waitForExit = true)
        {
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;

            var list = new List<string>();
            var process = new Process();
            process.StartInfo = processStartInfo;

            process.OutputDataReceived += (sender, data) => list.Add(data.Data);
            process.ErrorDataReceived += (sender, data) => list.Add(data.Data);

            var startStringForBash = waitForExit ? "" : "start "; // In order to execute commands in bash async, add start in the beginning
            Logger.Log(LogType.Log, $"Executing process: {processStartInfo.FileName} with arguments: {processStartInfo.Arguments}",
                $"Bash: {startStringForBash}cmd /c {(processStartInfo.FileName.Quated() +" " + processStartInfo.Arguments.Quated()).Quated()}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
                process.WaitForExit();

            var result = string.Join(Environment.NewLine, list);
            return result.Trim('\n', ' ', '\r');
        }

        public static string RunAsPython(string command)
        {
            var pythonFileName = "lastPythonCommand.py";
            try
            {
                File.WriteAllText(pythonFileName, command);
                return StartFromCommandLine("python", pythonFileName);
            }
            finally
            {
                if (File.Exists(pythonFileName))
                    File.Delete(pythonFileName);
            }
        }
    }
}
