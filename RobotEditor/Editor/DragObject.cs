using System;

namespace RobotEditor.Editor
{
    class DragObject
    {
        public object Sender { get; private set; }
        public object Value { get; private set; }
        public Action CallbackToSender { get; private set; }
    }
}
