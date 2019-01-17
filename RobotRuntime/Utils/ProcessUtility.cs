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
        public static string StartFromCommandLine(string executablePath, string arguments, ProcessStartInfo processStartInfo)
        {
            processStartInfo.FileName = executablePath;
            processStartInfo.Arguments = arguments;
            return StartFromCommandLine(processStartInfo);
        }

        public static string StartFromCommandLine(string executablePath, ProcessStartInfo processStartInfo)
        {
            processStartInfo.FileName = executablePath;
            return StartFromCommandLine(processStartInfo);
        }

        public static string StartFromCommandLine(string executablePath, string arguments)
        {
            return StartFromCommandLine(new ProcessStartInfo(executablePath, arguments));
        }

        public static string StartFromCommandLine(string executablePath)
        {
            return StartFromCommandLine(new ProcessStartInfo(executablePath));
        }

        public static string StartFromCommandLine(ProcessStartInfo processStartInfo)
        {
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = Environment.CurrentDirectory;

            var list = new List<string>();
            var process = Process.Start(processStartInfo);
            process.OutputDataReceived += (sender, data) => list.Add(data.Data);
            process.ErrorDataReceived += (sender, data) => list.Add(data.Data);

            // var stdout = process.StandardOutput.ReadToEnd();
            // var error = process.StandardError.ReadToEnd();

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
