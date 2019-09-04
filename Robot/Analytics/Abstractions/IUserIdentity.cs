namespace Robot.Analytics.Abstractions
{
    public interface IUserIdentity
    {
        string GetCpuID();
        string GetMacAddress();
        string GetMachineID();
        string GetOperatingSystem();
        string GetScreenResolution();
        string GetUserID();
        string GetCountryID();
    }
}
