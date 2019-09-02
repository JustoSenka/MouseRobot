using Robot.Abstractions;
using RobotRuntime;
using System.IO;
using Unity.Lifetime;

namespace Robot.Analytics
{
    public class ReceiveTrackingIDFromFile : IReceiveTrackingID
    {
        private string m_ID = "";
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(m_ID))
                    m_ID = File.ReadAllText("TrackingID.txt").Trim().Normalize();

                return m_ID;
            }
        }
    }
}
