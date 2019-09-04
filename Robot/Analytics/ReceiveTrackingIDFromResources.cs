using Robot.Abstractions;
using Robot.Analytics.Abstractions;
using RobotRuntime;
using System;
using System.Text;
using Unity.Lifetime;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(IReceiveTrackingID))]
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
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(k1));
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(k1));

                    var k2 = Properties.Resources.Secret3;

                    m_ID = Utils.Utils.GetDatum(k2, k1);
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
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(k1));
                    k1 = Encoding.UTF8.GetString(Convert.FromBase64String(k1));

                    var k2 = Properties.Resources.Secret2;

                    m_ID = Utils.Utils.GetDatum(k2, k1);
                }

                return m_ID;
            }
        }
    }
}
