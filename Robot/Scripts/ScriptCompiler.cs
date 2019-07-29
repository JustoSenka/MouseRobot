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
using Unity.Lifetime;

namespace Robot.Scripts
{
    /// <summary>
    /// ScriptCompiler lives in base robot domain. Its purpose is to compile files in project folder into user dlls.
    /// Used by PLuginManager
    /// </summary>
    [RegisterTypeToContainer(typeof(IScriptCompiler), typeof(ContainerControlledLifetimeManager))]
    public class ScriptCompiler : IScriptCompiler
    {
        public CSharpCodeProvider CodeProvider { get; private set; } = new CSharpCodeProvider();
        public CompilerParameters CompilerParams { get; private set; } = new CompilerParameters();

        public event Action ScriptsRecompiled;
        public bool IsCompiling => m_LastCompilationTask != null ? !m_LastCompilationTask.IsCompleted : false;

        private Task<bool> m_LastCompilationTask;
        private string m_OutputPath;

        private bool m_ShouldRecompile = false;
        private string[] m_TempSources;

        private object CompilationLock = new object();
        private object CompilationTaskLock = new object();

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
            CompilerParams.IncludeDebugInformation = true;
        }

        public Task<bool> CompileCode(params string[] sources)
        {
            lock (CompilationTaskLock)
            {
                if (IsCompiling)
                    return UpdateCompilationSources(sources);

                return m_LastCompilationTask = Task.Run(() =>
                {
                    return CompileCodeSync(sources);
                });
            }
        }

        public Task<bool> UpdateCompilationSources(params string[] sources)
        {
            if (Logger.AssertIf(!IsCompiling, "Cannot update compilation if not compiling. Might be race condition. Recompile manually"))
                return m_LastCompilationTask;

            m_ShouldRecompile = true;
            m_TempSources = sources;
            return m_LastCompilationTask;
        }

        private bool CompileCodeSync(string[] sources)
        {
            StatusManager.Add("ScriptCompiler", 2, new Status("", "Compiling...", StandardColors.Orange));

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()) + FileExtensions.DllD;

            CompilerResults results = null;
            lock (CompilationLock)
            {
                Profiler.Start("ScriptCompiler_CompileCode");

                CompilerParams.ReferencedAssemblies.Clear();
                CompilerParams.ReferencedAssemblies.AddRange(CompilerSettings.DefaultRobotReferences);
                CompilerParams.ReferencedAssemblies.AddRange(CompilerSettings.DefaultCompilerReferences);

                // Outputing to temp dir in case original files are being used right now, so compilation does not fail
                CompilerParams.OutputAssembly = tempPath;

                var settings = SettingsManager.GetSettings<CompilerSettings>();
                if (settings != null)
                {
                    if (settings.CompilerReferencesFromProjectFolder.Length > 0)
                        CompilerParams.ReferencedAssemblies.AddRange(settings.CompilerReferencesFromProjectFolder);

                    if (settings.HasValidReferences)
                        CompilerParams.ReferencedAssemblies.AddRange(settings.NonEmptyCompilerReferences);
                }
                else
                    Logger.Logi(LogType.Error, "CompilerSettings is null. It should not be. Compiler references cannot be added due to this. Please report a bug.");

                try
                {
                    results = CodeProvider.CompileAssemblyFromFile(CompilerParams, sources);
                }
                catch (FileNotFoundException fe)
                {
                    Logger.Logi(LogType.Error, "Roslyn compiler could not be found, did the build process failed to copy it? " + fe.Message);
                }
                catch (Exception e)
                {
                    Logger.Logi(LogType.Error, "Roslyn compiler threw an exception: " + e.Message);
                }

                Profiler.Stop("ScriptCompiler_CompileCode");
            }

            // This might happen if scripts are modified before compilation is finished
            if (m_ShouldRecompile)
            {
                m_ShouldRecompile = false;
                return CompileCodeSync(m_TempSources);
            }

            if (results != null && !results.Errors.HasErrors)
                ReplaceOldAssembly(tempPath, m_OutputPath);

            return PrintErrors(results);
        }

        private bool PrintErrors(CompilerResults results)
        {
            if (results == null)
            {
                Logger.Logi(LogType.Error, "Compilation failed due to Roslyn Compiler exception.");
                StatusManager.Add("ScriptCompiler", 8, new Status("", "Compilation Failed", StandardColors.Red));
                return false;
            }
            else if (results.Errors.HasErrors)
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

        /// <summary>
        /// Replaces old Dll and Pdb files with newly compiled ones. 
        /// </summary>
        private static void ReplaceOldAssembly(string tempPath, string outputPath)
        {
            var pdbTempPath = Path.Combine(Path.GetDirectoryName(tempPath), Path.GetFileNameWithoutExtension(tempPath)) + ".pdb";
            var pdbOutputPath = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath)) + ".pdb";

            if (File.Exists(tempPath))
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                File.Move(tempPath, outputPath);
            }

            if (File.Exists(pdbTempPath))
            {
                if (File.Exists(pdbOutputPath))
                    File.Delete(pdbOutputPath);

                File.Move(pdbTempPath, pdbOutputPath);
            }
        }

        public void SetOutputPath(string customAssemblyPath)
        {
            m_OutputPath = customAssemblyPath;
        }
    }
}
