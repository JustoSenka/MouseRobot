using Robot.Assets.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Assets.Analytics
{
    [RegisterTypeToContainer(typeof(ITestFixtureStructureAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class TestFixtureStructureAnalytics : ITestFixtureStructureAnalytics
    {
        private readonly IAnalytics Analytics;
        public TestFixtureStructureAnalytics(IAnalytics Analytics)
        {
            this.Analytics = Analytics;
        }

        public Task CountAndReportTestFixtureStructure(LightTestFixture fix)
        {
            return Task.Run(() =>
            {
                Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_FixtureStructure, AnalyticsEvent.L_TotalTestCount, fix.Tests.Count());

                Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_FixtureStructure, fix.Setup.Name, fix.Setup.Commands.GetAllNodes(false).Count());
                Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_FixtureStructure, fix.TearDown.Name, fix.TearDown.Commands.GetAllNodes(false).Count());
                Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_FixtureStructure, fix.OneTimeSetup.Name, fix.OneTimeSetup.Commands.GetAllNodes(false).Count());
                Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_FixtureStructure, fix.OneTimeTeardown.Name, fix.OneTimeTeardown.Commands.GetAllNodes(false).Count());

                foreach (var t in fix.Tests)
                    Analytics.PushEvent(AnalyticsEvent.K_AssetManager, AnalyticsEvent.A_FixtureStructure, t.Name, t.Commands.GetAllNodes(false).Count());
            });
        }
    }
}
