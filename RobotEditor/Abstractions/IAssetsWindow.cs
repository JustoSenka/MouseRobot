using System;
using Robot;
using System.Windows.Forms;

namespace RobotEditor.Abstractions
{
    public interface IAssetsWindow
    {
        event Action AssetSelected;

        Asset GetSelectedAsset();
        void AddMenuItemsForScriptTemplates(ToolStrip menuStrip, string menuItemName);
    }
}