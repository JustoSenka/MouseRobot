using Robot.Abstractions;
using RobotRuntime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class GoogleAnalytics : IAnalytics
    {
        private HttpClient m_Client;
        private HttpRequestMessage m_Request;
        private string m_MachineID;

        private IReceiveTrackingID IDReceiver;
        private IUserIdentity UserIdentity;
        public GoogleAnalytics(IReceiveTrackingID IDReceiver, IUserIdentity UserIdentity)
        {
            this.IDReceiver = IDReceiver;
            this.UserIdentity = UserIdentity;

            m_Client = new HttpClient();
            m_Client.BaseAddress = new Uri(@"https://www.google-analytics.com");
            m_Request = new HttpRequestMessage(HttpMethod.Post, "/collect");

            m_MachineID = UserIdentity.GetMachineID();
        }

        public Task<bool> PushEvent(string category, string action, string label, int value = 0)
        {
            return Task.Run(async () =>
            {
                m_Request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "v", "1" },
                    { "t", "event" },
                    { "tid", IDReceiver.ID},
                    { "cid", m_MachineID },
                    { "ec", category },
                    { "ea", action },
                    { "el", label },
                    { "ev", value + "" },
                });

                var response = await m_Client.SendAsync(m_Request);
                return response.IsSuccessStatusCode;
            });
        }
    }
}
