using System.Net.Http;
using System.Threading.Tasks;

namespace Robot.Abstractions
{
    public interface IAnalytics
    {
        Task<HttpResponseMessage> PushEvent(string category, string action, string label, int value = 0);
    }
}
