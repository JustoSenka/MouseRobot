using System.Collections.Generic;
using System.Threading.Tasks;

namespace Robot.Assets.Abstractions
{
    public interface IAssetManagerStartupAnalytics
    {
        Task CountAndReportAssetTypes(IEnumerable<Asset> Assets);
    }
}
