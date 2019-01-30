﻿using System.ComponentModel;
using System.Linq;
using System.Drawing;
using Robot.Abstractions;

namespace RobotEditor.PropertyUtils
{
    /// <summary>
    /// This will add a selection dropdown for Asset in property view
    /// </summary>
    public class AssetGUIDImageStringConverter : StringConverter
    {
        private static IAssetManager AssetManager { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(AssetManager.Assets.Where(a => a.HoldsTypeOf(typeof(Bitmap))).Select(a => a.Path).ToList());
        }
    }
}
