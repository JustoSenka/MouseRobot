using System.Threading.Tasks;

namespace Robot.Analytics.Abstractions
{
    public interface IUserIdentity
    {
        Task<string> GetMachineID();
        Task<string> GetUserIP();
        Task<string> GetCountryID();

        string GetCpuID();
        string GetMacAddress();
        string GetOperatingSystem();
        string GetScreenResolution();
        string GetUserID();
    }
}
