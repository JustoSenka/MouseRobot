using RobotEditor.Windows.Base;

namespace RobotEditor.Abstractions
{
    public interface IScreenPaintForm
    {
        void SubscribeToAllPainters();
        void AddClassToInvalidateList(IPaintOnScreen instance);
        void RemoveClassFromInvalidateList(IPaintOnScreen instance);
    }
}