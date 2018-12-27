﻿using System;
using System.Collections.Generic;

namespace Robot.Abstractions
{
    /// <summary>
    /// Subscribes to all AssetManager callbacks and collects all modified assets in a list, fires callback after refresh.
    /// This class can be Injected as Dependency for other managers.
    /// </summary>
    public interface IModifiedAssetCollector
    {
        /// <summary>
        /// Assets will filtered by extension. Only matching assets will be added to Paths. 
        /// No callbacks will be fired if no matching assets exists.
        /// </summary>
        IList<string> ExtensionFilters { get; }

        /// <summary>
        /// Paths of the assets modified after last refresh or asset addition/deletion has been made
        /// </summary>
        IList<string> Paths { get; }

        /// <summary>
        /// Fires callback after asset refresh or modification. Gives all paths of modified assets.
        /// Clears the list after callback was fired
        /// </summary>
        event Action<IList<string>> AssetsModified;
    }
}