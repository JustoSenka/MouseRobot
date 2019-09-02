using Robot.Abstractions;
using RobotRuntime;
using System;
using System.Text;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IReceiveTrackingID), typeof(ContainerControlledLifetimeManager))]
    public class ReceiveTrackingIDFromResources : IReceiveTrackingID
    {
        private string m_ID = "";
        public string ID
        {
            get
            {
                if (m_ID.IsEmpty())
                {
                    var k1 = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Resources.Secret1));
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Resources.Secret1));
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Resources.Secret1));

                    var k2 = Properties.Resources.Secret3;

                    m_ID = Utils.Utils.GetDatum(k1, k2);
                }

                return m_ID;
            }
        }
    }

    public class ReceiveTrackingIDFromResourcesForTesting : IReceiveTrackingID
    {
        private string m_ID = "";
        public string ID
        {
            get
            {
                if (m_ID.IsEmpty())
                {
                    var k1 = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Resources.Secret1));
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Resources.Secret1));
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Resources.Secret1));

                    var k2 = Properties.Resources.Secret2;

                    m_ID = Utils.Utils.GetDatum(k1, k2);
                }

                return m_ID;
            }
        }
    }
}
