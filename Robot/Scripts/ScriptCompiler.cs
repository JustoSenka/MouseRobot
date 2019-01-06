using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Robot.Abstractions;
using Robot.Settings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotRuntime.Utils;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Robot.Scripts
{
    /// <summary>
    /// ScriptCompiler lives in base robot domain. Its purpose is to compile files in project folder into user dlls.
    /// Used by PLuginManager
    /// </summary>
    public class ScriptCompiler : IScriptCompiler
    {
        public CSharpCodeProvider CodeProvider { get; private set; } = new CSharpCodeProvider();
        public CompilerParameters CompilerParams { get; private set; } = new CompilerParameters();

        public event Action ScriptsRecompiled;

        public bool IsCompiling { get; private set; } = false;
        public Task<bool> m_LastCompilationTask;

        private bool m_ShouldRecompile = false;
        private string[] m_TempSources;

        private IProfiler Profiler;
        private IStatusManager StatusManager;
        private ISettingsManager SettingsManager;
        private ILogger Logger;
        public ScriptCompiler(IProfiler Profiler, IStatusManager StatusManager, ISettingsManager SettingsManager, ILogger Logger)
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

        public Task<bool> CompileCode(params string[] sources)
        {
            if (IsCompiling)
            {
                // If already compiling, save sources for future compilation
                m_ShouldRecompile = true;
                m_TempSources = sources;
                return m_LastCompilationTask;
            }

            return m_LastCompilationTask = Task.Run(() =>
            {
                IsCompiling = true;
                return CompileCodeSync(sources);
            });
        }

        private bool CompileCodeSync(string[] sources)
        {
            StatusManager.Add("ScriptCompiler", 2, new Status("", "Compiling...", StandardColors.Orange));

            Profiler.Start("ScriptCompiler_CompileCode");

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
            Profiler.Stop("ScriptCompiler_CompileCode");

            // This might happen if scripts are modified before compilation is finished
            if (m_ShouldRecompile)
            {
                m_ShouldRecompile = false;
                return CompileCodeSync(m_TempSources);
            }

            IsCompiling = false;

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                    Logger.Logi(LogType.Error,
                        string.Format("({0}): {1}", error.ErrorNumber, error.ErrorText),
                        string.Format("at {0} {1} : {2}", error.FileName, error.Line, error.Column));

                ScriptsRecompiled?.Invoke();
                Logger.Logi(LogType.Error, "Scripts have compilation errors.");
                StatusManager.Add("ScriptCompiler", 8, new Status("", "Compilation Failed", StandardColors.Red));
                return false;
            }
            else
            {
                ScriptsRecompiled?.Invoke();
                Logger.Logi(LogType.Log, "Scripts successfully compiled.");
                StatusManager.Add("ScriptCompiler", 10, new Status("", "Compilation Complete", default));
                return true;
            }
        }

        public void SetOutputPath(string customAssemblyPath)
        {
            CompilerParams.OutputAssembly = customAssemblyPath;
        }
    }
}
