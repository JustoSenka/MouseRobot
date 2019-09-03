using Robot.Abstractions;
using Robot.Analytics.Abstractions;
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
        private readonly HttpClient m_Client;
        private readonly string m_MachineID;
        private bool m_FirstMessageSent = false;

        private readonly IReceiveTrackingID IDReceiver;
        private readonly IUserIdentity UserIdentity;
        public GoogleAnalytics(IReceiveTrackingID IDReceiver, IUserIdentity UserIdentity)
        {
            this.IDReceiver = IDReceiver;
            this.UserIdentity = UserIdentity;

            m_Client = new HttpClient();
            m_Client.BaseAddress = new Uri(@"https://www.google-analytics.com");

            m_MachineID = UserIdentity.GetMachineID();
        }

        public Task<bool> PushEvent(string category, string action, string label, int value = 0)
        {
            return Task.Run(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/collect");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
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

                var success = false;
                try
                {
                    var response = await m_Client.SendAsync(request);
                    success = response.IsSuccessStatusCode;

                    if (!m_FirstMessageSent && success)
                        Logger.Log(LogType.Debug, "Google Analytics connected.");

                    else if (!m_FirstMessageSent && !success)
                        Logger.Log(LogType.Debug, "Google Analytics messages are not going through.");
                }
                catch (Exception e)
                {
                    if (!m_FirstMessageSent)
                        Logger.Log(LogType.Debug, "Google Analytics threw an exception: " + e.Message);
                }

                m_FirstMessageSent = true;
                return success;
            });
        }
    }
}
