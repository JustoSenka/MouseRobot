using System.Threading.Tasks;

namespace Robot.Abstractions
{
    public interface IScriptManager
    {
        Task<bool> CompileScriptsAndReloadUserDomain();
    }
}