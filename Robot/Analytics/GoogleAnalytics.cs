using Robot.Analytics.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class GoogleAnalytics : IAnalytics
    {
        public bool IsEnabled { get; private set; } = true;

        private readonly HttpClient m_Client;
        private bool m_FirstMessageSent = false;

        private string m_MachineID;
        private string m_CountryID;
        private string m_IP;

        private Task m_InitializationTask;

        private int m_SentEvents;
        private int m_FailedEvents;

        private const string k_GoogleAnalyticsDomain = @"https://www.google-analytics.com";
        private const string k_GoogleAnalyticsCollectControler = @"/collect";

        private const int k_FailedEventThesholdToDisableAnalytics = 50;

        private readonly IReceiveTrackingID IDReceiver;
        private readonly IUserIdentity UserIdentity;
        public GoogleAnalytics(IReceiveTrackingID IDReceiver, IUserIdentity UserIdentity)
        {
            this.IDReceiver = IDReceiver;
            this.UserIdentity = UserIdentity;

            m_Client = new HttpClient();
            m_Client.BaseAddress = new Uri(k_GoogleAnalyticsDomain);

            try
            {
                // Run initialization async since it is created on startup and getting IPs and MachineID is pretty slow.
                // All analytics events will wait for this task to finish before sending any.
                m_InitializationTask = Task.Run(async () =>
                {
                    m_IP = await UserIdentity.GetUserIP();
                    m_CountryID = await UserIdentity.GetCountryID();
                    m_MachineID = await UserIdentity.GetMachineID();
                }, new CancellationTokenSource(15000).Token);

                m_InitializationTask.ContinueWith((t) => m_InitializationTask = null);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Analytics timeout on startup setting up user ID: " + e.Message);
            }
        }

        public Task<bool> PushEvent(string category, string action, string label, int value = 0)
        {
            return Task.Run(async () =>
            {
                await WaitForInitializationIfNotYetCompleted();

                var request = new HttpRequestMessage(HttpMethod.Post, k_GoogleAnalyticsCollectControler);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "v", "1" },
                    { "t", "event" },

                    { "tid", IDReceiver.ID},
                    { "uip", m_IP},
                    { "geoid", m_CountryID},
                    { "cid", m_MachineID },
                    { "sr", UserIdentity.GetScreenResolution() },
                    { "ul", RegionInfo.CurrentRegion.Name },
                    // { "ds", new DirectoryInfo(Environment.CurrentDirectory).Name },

                    { "dl", new DirectoryInfo(Environment.CurrentDirectory).Name }, // PAGE NAME as Project name
                    // { "dt", new DirectoryInfo(Environment.CurrentDirectory).Name }, // PAGE Title

                    { "ec", category }, // Category - AssetManager etc.
                    { "ea", action }, // Action as ClickMenuItem
                    { "el", label }, // Label as Which specific menu item
                    { "ev", value + "" }, // Either value or conditions as did menu item click succeded 1 or 0

                    { "an", Paths.AppName},
                    { "av", FileVersionInfo.GetVersionInfo(Paths.ApplicationExecutablePath).ProductVersion},
#if DEBUG 
                    { "aid", "Debug"},
#else
                    { "aid", "Release"},
#endif

                    /*
                     * an ApplicationName
                     * aid ApplicationID
                     * av ApplicationVersion
                     * aiid ApplicationInstallerID
                     * 
                     * 
                     * 
                     */
                });

                return await SendAnalyticsRequest(request);
            });
        }

        // -- Private --

        private async Task WaitForInitializationIfNotYetCompleted()
        {
            if (m_InitializationTask != null || !m_InitializationTask.IsCompleted)
                await m_InitializationTask;
        }

        private async Task<bool> SendAnalyticsRequest(HttpRequestMessage request)
        {
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

            if (success) m_SentEvents++; else m_FailedEvents++;

            // If constantly failing to send the events, disable analytics so systems know and stop calculating data for all these events
            if (m_FailedEvents > k_FailedEventThesholdToDisableAnalytics && m_SentEvents == 0)
                IsEnabled = false;

            return success;
        }
    }
}
