using System.Threading.Tasks;

namespace Robot.Abstractions
{
    public interface IScriptManager
    {
        Task<bool> CompileScriptsAndReloadUserDomain();

        bool AllowCompilation { get; set; }
        bool IsCompilingOrReloadingAssemblies { get; }
    }
}