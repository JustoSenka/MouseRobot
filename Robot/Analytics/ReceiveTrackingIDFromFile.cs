﻿using Robot.Analytics.Abstractions;
using System.IO;

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
