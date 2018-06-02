using RobotEditor.Hierarchy;

namespace RobotEditor.Abstractions
{
    public interface IHierarchyNodeStringConverter
    {
        string ToString(HierarchyNode node);
    }
}