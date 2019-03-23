using NUnit.Framework;

namespace Tests.Graphics
{
    [TestFixture]
    public class GraphicsTests : TestWithCleanup
    {
        [Test]
        public void FindImageInsideAnotherImage()
        {
            // ScreenShot and smaller screenshot

            /*
            MouseRobot.ForceInit();
            AssetManager.Refresh();

            var modelImage = new Mat(AssetManager.GetAsset("Images", "UnityButton").Path);
            var observedImage = new Mat(AssetManager.GetAsset("Images", "UnityCollab").Path);
            */
            //var avgPos = FeatureDetector.Get().FindImageRect(modelImage.Bitmap, observedImage.Bitmap);

            //Assert.IsNotNull(avgPos);
            //Assert.AreNotEqual(0, avgPos.Length);
        }
    }
}
