using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace Robot.Abstractions
{
    public interface IPluginCompiler
    {
        CSharpCodeProvider CodeProvider { get; }
        CompilerParameters CompilerParams { get; }

        void AddReferencedAssemblies(params string[] paths);
        bool CompileCode(string code);
        void SetOutputPath(string customAssemblyPath);
    }
}