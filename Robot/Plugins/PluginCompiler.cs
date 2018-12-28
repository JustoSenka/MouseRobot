using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Robot.Abstractions;
using Robot.Settings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotRuntime.Utils;
using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Robot.Plugins
{
    /// <summary>
    /// PluginCompiler lives in base robot domain. Its purpose is to compile files in project folder into user dlls.
    /// Used by PLuginManager
    /// </summary>
    public class PluginCompiler : IPluginCompiler
    {
        public CSharpCodeProvider CodeProvider { get; private set; } = new CSharpCodeProvider();
        public CompilerParameters CompilerParams { get; private set; } = new CompilerParameters();

        public event Action ScriptsRecompiled;

        private bool m_IsCompiling = false;
        private bool m_ShouldRecompile = false;
        private string[] m_TempSources;

        private IProfiler Profiler;
        private IStatusManager StatusManager;
        private ISettingsManager SettingsManager;
        private ILogger Logger;
        public PluginCompiler(IProfiler Profiler, IStatusManager StatusManager, ISettingsManager SettingsManager, ILogger Logger)
        {
            this.Profiler = Profiler;
            this.StatusManager = StatusManager;
            this.SettingsManager = SettingsManager;
            this.Logger = Logger;
            
            // hack to change path
            var settings = typeof(CSharpCodeProvider)
                .GetField("_compilerSettings", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(CodeProvider);

            settings.GetType()
                .GetField("_compilerFullPath", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(settings, Path.Combine(Paths.ApplicationInstallPath, "roslyn", "csc.exe"));
            

            CompilerParams.GenerateExecutable = false;
            CompilerParams.GenerateInMemory = false;
        }

        public void CompileCode(params string[] sources)
        {
            // If already compiling, save sources for future compilation
            if (m_IsCompiling)
            {
                m_ShouldRecompile = true;
                m_TempSources = sources;
                return;
            }

            m_IsCompiling = true;

            new Thread(() => CompileCodeSync(sources)).Start();
        }

        private bool CompileCodeSync(string[] sources)
        {
            StatusManager.Add("PluginCompiler", 2, new Status("", "Compiling...", StandardColors.Orange));

            Profiler.Start("PluginCompiler_CompileCode");

            CompilerParams.ReferencedAssemblies.Clear();
            CompilerParams.ReferencedAssemblies.AddRange(CompilerSettings.DefaultRobotReferences);
            CompilerParams.ReferencedAssemblies.AddRange(CompilerSettings.DefaultCompilerReferences);

            var settings = SettingsManager.GetSettings<CompilerSettings>();
            if (settings != null)
            {
                if (settings.HasValidReferences)
                    CompilerParams.ReferencedAssemblies.AddRange(settings.NonEmptyCompilerReferences);
            }
            else
                Logger.Logi(LogType.Error, "CompilerSettings is null. It should not be. Compiler references cannot be added due to this. Please report a bug.");

            var results = CodeProvider.CompileAssemblyFromSource(CompilerParams, sources);
            Profiler.Stop("PluginCompiler_CompileCode");

            m_IsCompiling = false;

            // This might happen if scripts are modified before compilation is finished
            if (m_ShouldRecompile)
            {
                m_ShouldRecompile = false;
                CompileCode(m_TempSources);
                return false;
            }

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                    Logger.Logi(LogType.Error,
                        string.Format("({0}): {1}", error.ErrorNumber, error.ErrorText),
                        string.Format("at {0} {1} : {2}", error.FileName, error.Line, error.Column));

                ScriptsRecompiled?.Invoke();
                Logger.Logi(LogType.Error, "Scripts have compilation errors.");
                StatusManager.Add("PluginCompiler", 8, new Status("", "Compilation Failed", StandardColors.Red));
                return false;
            }
            else
            {
                ScriptsRecompiled?.Invoke();
                Logger.Logi(LogType.Log, "Scripts successfully compiled.");
                StatusManager.Add("PluginCompiler", 10, new Status("", "Compilation Complete", default));
                return true;
            }

        }

        public void SetOutputPath(string customAssemblyPath)
        {
            CompilerParams.OutputAssembly = customAssemblyPath;
        }
    }
}
