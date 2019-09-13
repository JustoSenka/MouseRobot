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

        public static string A_ProjectStructure => "ProjectStructure";
        public static string A_RecordingStructure => "RecordingStructure";
        public static string A_FixtureStructure => "FixtureStructure";

        public static string A_Create => "Create";
        public static string A_Duplicate => "Duplicate";
        public static string A_Delete => "Delete";
        public static string A_Load => "Load";
        public static string A_Save => "Save";
        public static string A_SetActive => "SetActive";

        public static string A_MenuItemClick => "MenuItemClick";

        public static string L_Selection => "Selection";
        public static string L_TotalAssetCount => "TotalAssetCount";
        public static string L_TotalCommandCount => "TotalCommandCount";
        public static string L_TotalTestCount => "TotalTestCount";
    }
}
