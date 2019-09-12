using Robot.Assets.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Assets.Analytics
{
    [RegisterTypeToContainer(typeof(IRecordingStructureAnalytics), typeof(ContainerControlledLifetimeManager))]
    public class RecordingStructureAnalytics : IRecordingStructureAnalytics
    {
        private readonly IAnalytics Analytics;
        public RecordingStructureAnalytics(IAnalytics Analytics)
        {
            this.Analytics = Analytics;
        }

        public Task CountAndReportRecordingStructure(string category, Recording rec)
        {
            return Task.Run(() =>
            {
                var map = new Dictionary<Type, int>();

                foreach (var c in rec.Commands.GetAllNodes())
                {
                    var type = c.value.GetType();
                    if (!map.ContainsKey(type))
                        map.Add(type, 1);
                    else
                        map[type]++;
                }

                Analytics.PushEvent(category, AnalyticsEvent.A_RecordingStructure, AnalyticsEvent.L_TotalCommandCount, rec.Commands.GetAllNodes().Count());

                foreach (var type in map.Keys)
                    Analytics.PushEvent(category, AnalyticsEvent.A_RecordingStructure, type.Name, map[type]);
            });
        }
    }
}
