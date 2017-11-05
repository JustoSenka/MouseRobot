using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.CV;
using Emgu.CV.Util;
using RobotRuntime.Graphics;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System.Drawing;
using Robot;
using System.Collections.Generic;
using System.Linq;
using RobotRuntime.Utils;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;

namespace Tests
{
    [TestClass]
    public class GraphicsTests
    {
        [TestMethod]
        public void FindImageInsideAnotherImage()
        {
            MouseRobot.Instance.ForceInit();
            AssetManager.Instance.Refresh();

            var modelImage = new Mat(AssetManager.Instance.GetAsset("Images", "UnityButton").Path);
            var observedImage = new Mat(AssetManager.Instance.GetAsset("Images", "UnityCollab").Path);

            long time;
            var avgPos = FeatureDetection.FindImagePos(modelImage, observedImage, out time);

            Assert.IsNotNull(avgPos);
            Assert.AreNotEqual(0, avgPos.Length);
        }
    }
}
