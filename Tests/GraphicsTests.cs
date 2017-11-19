using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.CV;
using RobotRuntime.Graphics;
using Robot;

namespace Tests
{
    [TestClass]
    public class GraphicsTests
    {
        [TestMethod]
        public void FindImageInsideAnotherImage()
        {
            /*
            MouseRobot.Instance.ForceInit();
            AssetManager.Instance.Refresh();

            var modelImage = new Mat(AssetManager.Instance.GetAsset("Images", "UnityButton").Path);
            var observedImage = new Mat(AssetManager.Instance.GetAsset("Images", "UnityCollab").Path);
            */
            //var avgPos = FeatureDetector.Get().FindImageRect(modelImage.Bitmap, observedImage.Bitmap);

            //Assert.IsNotNull(avgPos);
            //Assert.AreNotEqual(0, avgPos.Length);
        }
    }
}
