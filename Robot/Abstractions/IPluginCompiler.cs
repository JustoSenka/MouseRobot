using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System;

namespace Robot.Abstractions
{
    public interface IPluginCompiler
    {
        CSharpCodeProvider CodeProvider { get; }
        CompilerParameters CompilerParams { get; }

        void AddReferencedAssemblies(params string[] paths);
        bool CompileCode(params string[] sources);
        void SetOutputPath(string customAssemblyPath);

        event Action ScriptsRecompiled;
    }
}