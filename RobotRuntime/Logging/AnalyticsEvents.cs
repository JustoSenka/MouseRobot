using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public static class AnalyticsEvent
    {
        public static string K_AssetManager => "AssetManager";
        public static string K_Hierarchy => "Hierarchy"; // Use GetType().Name for BaseHierarchyWindows

        public static string A_AssetTypes => "AssetTypes";
        public static string A_Create => "Create";
        public static string A_Duplicate => "Duplicate";
        public static string A_Delete => "Delete";
        public static string A_Load => "Load";

        public static string A_Expand => "Expand";
        public static string A_Collapse => "Collapse";

        public static string L_Selection => "Selection";
        public static string L_TotalAssetCount => "TotalAssetCount";

        public static string L_One => "One";
        public static string L_All => "All";
    }
}
