using Robot.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RobotRuntime;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class GoogleAnalytics :IAnalytics
    {
        private IReceiveTrackingID IDReceiver;
        private IUserIdentity UserIdentity;
        public GoogleAnalytics(IReceiveTrackingID IDReceiver, IUserIdentity UserIdentity)
        {
            this.IDReceiver = IDReceiver;
            this.UserIdentity = UserIdentity;
        }

        public Task<HttpResponseMessage> PushEvent(string category, string action, string label, int value = 0)
        {
            using (var client = new HttpClient())
            {
                var url = "www.google-analytics.com";
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                   { "v", "1" },
                   { "t", "event" },
                   { "tid", IDReceiver.ID },
                   { "cid", UserIdentity.GetMachineID() },
                   { "ec", category },
                   { "ea", action },
                   { "el", label },
                   { "ev", value + "" },
                });

                return client.PostAsync(url, content);
            }
        }
    }
}
