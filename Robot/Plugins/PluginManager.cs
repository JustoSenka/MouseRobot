using Microsoft.CSharp;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Robot.Plugins
{
    public class PluginManager
    {
        private CSharpCodeProvider m_CodeProvider = new CSharpCodeProvider();
        private CompilerParameters m_CompilerParams = new CompilerParameters();

        private IList<string> m_ModifiedFilesSinceLastRecompilation = new List<string>();

        private string DomainName { get { return "UserScripts"; } }

        private string CustomAssemblyName { get { return "CustomAssembly.dll"; } }
        private string CustomAssemblyPath { get { return Path.Combine(Paths.MetadataPath, CustomAssemblyName); } }

        public AppDomain PluginDomain { get; private set; }

        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public PluginManager(IAssetManager AssetManager, IProfiler Profiler)
        {
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.RefreshFinished += RecompileScripts;

            m_CompilerParams.GenerateExecutable = false;
            m_CompilerParams.GenerateInMemory = false;

            //PluginDomain = AppDomain.CreateDomain(DomainName);

            AddReferencesForCompilerParameter();
        }

        private void AddReferencesForCompilerParameter()
        {
            var assemblies = GetAllAssembliesInDomainDirectory();
            m_CompilerParams.ReferencedAssemblies.AddRange(assemblies.Distinct().ToArray());

            m_CompilerParams.ReferencedAssemblies.Add("System.dll");
            m_CompilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
            m_CompilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
        }

        private IEnumerable<string> GetAllAssembliesInDomainDirectory()
        {
            foreach (var path in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (path.EndsWith(".dll") || path.EndsWith(".exe"))
                    yield return path;
            }
        }

        private void AddPathToList(string assetPath)
        {
            if (assetPath.EndsWith(FileExtensions.PluginD))
                m_ModifiedFilesSinceLastRecompilation.Add(assetPath);
        }

        private void RecompileScripts()
        {
            if (m_ModifiedFilesSinceLastRecompilation.Count == 0)
                return;

            m_CompilerParams.OutputAssembly = CustomAssemblyPath;

            Profiler.Start("PluginManager_RecompileScripts");

            var scriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD));
            var scriptValues = scriptAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            foreach (var s in scriptValues)
            {
                CompileCode(s);
            }

            Profiler.Stop("PluginManager_RecompileScripts");
            Logger.Log(LogType.Log, "Scripts successfully recompiled");

            m_ModifiedFilesSinceLastRecompilation.Clear();
        }

        private Assembly CompileCode(string code)
        {
            var results = m_CodeProvider.CompileAssemblyFromSource(m_CompilerParams, code);
            
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                    Logger.Log(LogType.Error, 
                        string.Format("({0}): {1}", error.ErrorNumber, error.ErrorText), 
                        string.Format("at {0} {1} : {2}", error.FileName, error.Line, error.Column));

                return null;
            }

            return results.CompiledAssembly;
        }
    }
}
