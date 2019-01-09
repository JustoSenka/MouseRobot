using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Threading.Tasks;

namespace Robot.Abstractions
{
    public interface IScriptCompiler
    {
        CSharpCodeProvider CodeProvider { get; }
        CompilerParameters CompilerParams { get; }

        bool IsCompiling { get; }

        Task<bool> CompileCode(params string[] sources);
        Task<bool> UpdateCompilationSources(params string[] sources);

        void SetOutputPath(string customAssemblyPath);

        event Action ScriptsRecompiled;
    }
}