using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;

namespace Robot.Abstractions
{
    public interface IPluginCompiler
    {
        CSharpCodeProvider CodeProvider { get; }
        CompilerParameters CompilerParams { get; }

        void CompileCode(params string[] sources);
        void SetOutputPath(string customAssemblyPath);

        event Action ScriptsRecompiled;
    }
}