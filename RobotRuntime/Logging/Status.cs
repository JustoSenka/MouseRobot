using System.Drawing;

namespace RobotRuntime.Logging
{
    public struct Status
    {
        public string EditorStatus { get; set; }
        public string CurrentOperation { get; set; }
        public Color Color { get; set; }

        public Status(string EditorStatus, string CurrentOperation, Color Color) 
        {
            this.EditorStatus = EditorStatus;
            this.CurrentOperation = CurrentOperation;
            this.Color = Color;
        }

        public static bool operator ==(Status s1, Status s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(Status s1, Status s2)
        {
            return !s1.Equals(s2);
        }

        public bool Equals(Status other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var s = (Status)obj;
            return s.Color == Color && s.CurrentOperation == CurrentOperation && s.EditorStatus == EditorStatus;
        }

        public override int GetHashCode()
        {
            var sum = Color.GetHashCode() + CurrentOperation.GetHashCode() + EditorStatus.GetHashCode();
            return sum.GetHashCode();
        }
    }
}
