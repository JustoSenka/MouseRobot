using Robot.Abstractions;
using Robot.Assets.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Assets
{
    [RegisterTypeToContainer(typeof(IAssetManagerStartupAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class AssetManagerStartupAnalytics : IAssetManagerStartupAnalytics
    {
        private readonly IAnalytics Analytics;
        public AssetManagerStartupAnalytics(IAnalytics Analytics)
        {
            this.Analytics = Analytics;
        }

        public Task CountAndReportAssetTypes(IEnumerable<Asset> Assets)
        {
            return Task.Run(() =>
            {
                var map = new Dictionary<Type, int>();

                foreach (var a in Assets)
                {
                    var type = a.ImporterType;
                    if (!map.ContainsKey(type))
                        map.Add(type, 1);
                    else
                        map[type]++;
                }

                Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_AssetTypes, AnalyticsEvent.L_TotalAssetCount, Assets.Count());

                foreach (var type in map.Keys)
                    Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_AssetTypes, type.Name, map[type]);
            });
        }
    }
}
