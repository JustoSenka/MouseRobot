namespace Robot.Analytics.Abstractions
{
    public interface INetwork
    {
        string GetExternalIP();
        string GetLocalIP();
        string GetCountryID(string ip = "");
    }
}