using System;
using Robot;

namespace RobotEditor.Abstractions
{
    public interface IAssetsWindow
    {
        event Action AssetSelected;

        Asset GetSelectedAsset();
    }
}