using Robot.Assets.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Assets.Analytics
{
    [RegisterTypeToContainer(typeof(IAssetManagerStartupAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class AssetManagerStartupAnalytics : IAssetManagerStartupAnalytics
    {
        private readonly IAnalytics Analytics;
        private readonly IRecordingStructureAnalytics RecordingStructureAnalytics;
        private readonly ITestFixtureStructureAnalytics TestFixtureStructureAnalytics;
        private readonly IProfiler Profiler;

        public AssetManagerStartupAnalytics(IAnalytics Analytics, IRecordingStructureAnalytics RecordingStructureAnalytics,
            ITestFixtureStructureAnalytics TestFixtureStructureAnalytics, IProfiler Profiler)
        {
            this.Analytics = Analytics;
            this.RecordingStructureAnalytics = RecordingStructureAnalytics;
            this.TestFixtureStructureAnalytics = TestFixtureStructureAnalytics;
            this.Profiler = Profiler;
        }

        public Task CountAndReportAssetTypes(IEnumerable<Asset> Assets)
        {
            return Task.Run(() =>
            {
                Profiler.Begin("Analytics_ProjectStructure", () =>
                {
                    try
                    {
                        var map = new Dictionary<Type, int>();

                        foreach (var a in Assets)
                        {
                            var type = a.ImporterType;
                            if (!map.ContainsKey(type))
                                map.Add(type, 1);
                            else
                                map[type]++;

                            /* TODO:
                             * Disabled these analytics for now, because they load assets before any scripts have compiled
                             * which will results in custom commands failing to load.
                             * Will need to enable once startup sequence is refactored to allow such loads
                             
                            if (a.HoldsType() == typeof(Recording))
                                RecordingStructureAnalytics.CountAndReportRecordingStructure(a.Load<Recording>());
                            else if (a.HoldsType() == typeof(LightTestFixture))
                                TestFixtureStructureAnalytics.CountAndReportTestFixtureStructure(a.Load<LightTestFixture>());
                                */
                        }

                        Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_ProjectStructure, AnalyticsEvent.L_TotalAssetCount, Assets.Count());

                        foreach (var type in map.Keys)
                            Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_ProjectStructure, type.Name, map[type]);
                    }
                    catch(Exception e)
                    {
                        Logger.Log(LogType.Error, "Analytics threw an exception on asset manager initialize reporting: " + e.Message);
                    }
                });
            });
        }
    }
}
