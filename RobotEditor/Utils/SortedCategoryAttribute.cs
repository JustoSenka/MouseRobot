using System.ComponentModel;

namespace RobotEditor.Utils
{
    public class SortedCategoryAttribute : CategoryAttribute
    {
        private const char NonPrintableChar = '\t';

        public SortedCategoryAttribute(string category, ushort categoryPos, ushort totalCategories)
            : base(category.PadLeft(category.Length + (totalCategories - categoryPos), NonPrintableChar))
        {
        }
    }
}
