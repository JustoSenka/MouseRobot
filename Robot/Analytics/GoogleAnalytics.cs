using Robot.Abstractions;
using RobotRuntime;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class GoogleAnalytics : IAnalytics
    {
        private IReceiveTrackingID IDReceiver;
        private IUserIdentity UserIdentity;
        public GoogleAnalytics(IReceiveTrackingID IDReceiver, IUserIdentity UserIdentity)
        {
            this.IDReceiver = IDReceiver;
            this.UserIdentity = UserIdentity;
        }

        public Task<bool> PushEvent(string category, string action, string label, int value = 0)
        {
            return Task.Run(async () =>
            {
                using (var client = new HttpClient())
                {
                    var url = @"http://www.google-analytics.com";
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

                    var response = await client.PostAsync(url, content);
                    return response.IsSuccessStatusCode;
                }
            });
        }
    }
}
