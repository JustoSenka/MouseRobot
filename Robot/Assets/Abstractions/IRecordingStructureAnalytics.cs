using System.Threading.Tasks;
using RobotRuntime.Recordings;

namespace Robot.Assets.Abstractions
{
    public interface IRecordingStructureAnalytics
    {
        Task CountAndReportRecordingStructure(Recording rec);
    }
}
