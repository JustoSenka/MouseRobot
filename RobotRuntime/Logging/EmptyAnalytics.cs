using RobotRuntime.Abstractions;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace RobotRuntime.Logging
{
    /// <summary>
    /// This empty anlytics class is used only when running tests from command line. 
    /// When editor is open, runtime analytics will be overriden by one specified in Robot project.
    /// </summary>
    [RegisterTypeToContainer(typeof(IAnalytics), typeof(ContainerControlledLifetimeManager))]
    class EmptyAnalytics : IAnalytics
    {
        public bool IsEnabled => false;

        public Task<bool> PushEvent(string category, string action, string label, int value = 0)
        {
            return null;
        }
    }
}
