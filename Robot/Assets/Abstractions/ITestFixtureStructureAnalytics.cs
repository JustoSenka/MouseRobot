using System.Threading.Tasks;
using RobotRuntime.Tests;

namespace Robot.Assets.Abstractions
{
    public interface ITestFixtureStructureAnalytics
    {
        Task CountAndReportTestFixtureStructure(string category, LightTestFixture fix);
    }
}
