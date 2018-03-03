using Microsoft.CSharp;
using Robot.Abstractions;
using RobotRuntime;
using System.CodeDom.Compiler;

namespace Robot.Plugins
{
    public class PluginCompiler : IPluginCompiler
    {
        public CSharpCodeProvider CodeProvider { get; private set; } = new CSharpCodeProvider();
        public CompilerParameters CompilerParams { get; private set; } = new CompilerParameters();

        public PluginCompiler()
        {
            CompilerParams.GenerateExecutable = false;
            CompilerParams.GenerateInMemory = false;

            CompilerParams.ReferencedAssemblies.Add("System.dll");
            CompilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
            CompilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
        }

        public void AddReferencedAssemblies(params string[] paths)
        {
            CompilerParams.ReferencedAssemblies.AddRange(paths);
        }

        public bool CompileCode(string code)
        {
            var results = CodeProvider.CompileAssemblyFromSource(CompilerParams, code);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                    Logger.Log(LogType.Error,
                        string.Format("({0}): {1}", error.ErrorNumber, error.ErrorText),
                        string.Format("at {0} {1} : {2}", error.FileName, error.Line, error.Column));

                Logger.Log(LogType.Log, "Scripts have compilation errors.");
                return false;
            }

            Logger.Log(LogType.Log, "Script successfully compiled.");
            return true;
        }

        public void SetOutputPath(string customAssemblyPath)
        {
            CompilerParams.OutputAssembly = customAssemblyPath;
        }
    }
}
