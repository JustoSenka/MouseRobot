using System;
using Microsoft.CSharp;
using RobotRuntime;
using System.CodeDom.Compiler;
using System.Reflection;
using RobotRuntime.Abstractions;

public class PluginCompiler : MarshalByRefObject
{
    public CSharpCodeProvider CodeProvider { get; private set; } = new CSharpCodeProvider();
    public CompilerParameters CompilerParams { get; private set; } = new CompilerParameters();


    public Assembly LoadedAssembly;

    public ILogger Logger;
    public PluginCompiler()
    {
        CompilerParams.GenerateExecutable = false;
        CompilerParams.GenerateInMemory = false;
    }

    public void AddReferencedAssemblies(params string[] paths)
    {
        CompilerParams.ReferencedAssemblies.AddRange(paths);
        CompilerParams.ReferencedAssemblies.Add("System.dll");
        CompilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
        CompilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
    }

    public string CompileCode(string code)
    {
        var results = CodeProvider.CompileAssemblyFromSource(CompilerParams, code);

        if (results.Errors.HasErrors)
        {
            foreach (CompilerError error in results.Errors)
                Logger.Logi(LogType.Error,
                    string.Format("({0}): {1}", error.ErrorNumber, error.ErrorText),
                    string.Format("at {0} {1} : {2}", error.FileName, error.Line, error.Column));

            return null;
        }
        
        LoadedAssembly = results.CompiledAssembly;
        Logger.Logi(LogType.Log, "Script compiled and loaded in UserScripts Domain");

        return results.PathToAssembly;
    }

    public void SetOutputDirectory(string customAssemblyPath)
    {
        CompilerParams.OutputAssembly = customAssemblyPath;
    }
}

public class Proxy : MarshalByRefObject
{
    public void LoadAssemblies(string[] paths)
    {
        foreach (var path in paths)
        {
            try
            {
                Assembly.LoadFrom(path);
            }
            catch (Exception)
            {
                Console.WriteLine("Assembly could not be loaded: " + path);
            }
        }
    }
}