using System;
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
        /// Should asset lists be automatically cleared after callbacks were fired.
        /// </summary>
        bool AutoClear { get; set; }

        /// <summary>
        /// Assets will filtered by extension. Only matching assets will be added to Paths. 
        /// No callbacks will be fired if no matching assets exists.
        /// </summary>
        IList<string> ExtensionFilters { get; }

        /// <summary>
        /// Paths of the assets modified after last refresh or asset addition/deletion has been made
        /// </summary>
        IList<string> ModifiedAssetPaths { get; }

        /// <summary>
        /// Fires callback after asset refresh or modification. Gives all paths of modified assets.
        /// Clears the list after callback was fired
        /// </summary>
        event Action<IEnumerable<string>> AssetsModified;

        /// <summary>
        /// Paths of the assets which were renamed after last refresh or asset rename via AssetManager has been made
        /// </summary>
        IList<(string From, string To)> RenamedAssetPaths { get; }

        /// <summary>
        /// Fires callback after asset refresh or modification. Gives all paths of modified assets.
        /// Clears the list after callback was fired
        /// </summary>
        event Action<IEnumerable<(string From, string To)>> AssetsRenamed;

        /// <summary>
        /// Clears all the lists but keeps the filter
        /// </summary>
        void Clear();
    }
}
