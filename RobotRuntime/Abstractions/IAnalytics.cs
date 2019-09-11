using System.Net.Http;
using System.Threading.Tasks;

namespace RobotRuntime.Abstractions
{
    public interface IAnalytics
    {
        bool IsEnabled { get; }
        Task<bool> PushEvent(string category, string action, string label, int value = 0);
    }
}
