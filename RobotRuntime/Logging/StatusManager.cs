using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;

namespace RobotRuntime.Logging
{
    public class StatusManager : IStatusManager
    {
        private static readonly Status k_DefaultStatus = new Status("Ready", "", StandardColors.Blue);

        public Status Status { get { return m_StatusList.First(); } }

        private PriorityStack<Status> m_StatusList = new PriorityStack<Status>();
        private Dictionary<string, Status> m_StatusNameTable = new Dictionary<string, Status>();

        public event Action<Status> StatusUpdated;
        public event Action<Status> AnimationUpdated;

        public StatusManager()
        {
            Add(this.GetType().ToString(), 10, k_DefaultStatus);
        }

        public void Add(string uniqueName, int priority, Status newStatus)
        {
            newStatus = FixStatusDefaults(newStatus);

            if (m_StatusNameTable.ContainsKey(uniqueName))
            {
                m_StatusList.Remove(m_StatusNameTable[uniqueName]);
                m_StatusNameTable.Remove(uniqueName);
            }

            m_StatusList.Add(newStatus, priority);
            m_StatusNameTable.Add(uniqueName, newStatus);

            StatusUpdated?.Invoke(m_StatusList.First());
            AnimationUpdated?.Invoke(m_StatusList.First());
        }

        public void Remove(string uniqueName)
        {
            if (m_StatusNameTable.ContainsKey(uniqueName))
            {
                m_StatusList.Remove(m_StatusNameTable[uniqueName]);
                m_StatusNameTable.Remove(uniqueName);
                StatusUpdated?.Invoke(Status);
            }
        }

        private Status FixStatusDefaults(Status status)
        {
            if (status.IsDefault())
                status = k_DefaultStatus;

            if (status.EditorStatus == null || status.EditorStatus == "")
                status.EditorStatus = k_DefaultStatus.EditorStatus;

            if (status.CurrentOperation == null || status.CurrentOperation == "")
                status.CurrentOperation = k_DefaultStatus.CurrentOperation;

            if (status.Color.IsDefault())
                status.Color = k_DefaultStatus.Color;

            return status;
        }
    }
}
