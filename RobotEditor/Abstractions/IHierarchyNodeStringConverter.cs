using RobotEditor.Scripts;

namespace RobotEditor.Abstractions
{
    public interface IHierarchyNodeStringConverter
    {
        string ToString(HierarchyNode node);
    }
}