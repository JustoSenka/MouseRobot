using RobotRuntime.Abstractions;
using System.Drawing;
using System.Linq;
using Unity.Lifetime;

namespace RobotRuntime.Graphics
{
    [RegisterTypeToContainer(typeof(ITextDetectionManager), typeof(ContainerControlledLifetimeManager))]
    public class TextDetectionManager : BaseDetectionManager, ITextDetectionManager
    {
        protected override string DefaultDetector { get; set; } = "Tesseract";

        private readonly IFactoryWithCache<TextDetector> Factory;
        public TextDetectionManager(IFactoryWithCache<TextDetector> Factory)
        {
            this.Factory = Factory;
        }

        /// <summary>
        /// Returns array of positions for given image and detector.
        /// Always returns array even if Feature Detector supports only single images
        /// </summary>
        protected override bool FindImageRectsSync(Detectable detectable, string detectorName, Bitmap observedImage, out Point[][] points)
        {
            var m_FeatureDetector = Factory.Create(detectorName);
            if (m_FeatureDetector == null)
            {
                m_FeatureDetector = Factory.Create(DefaultDetector);
                if (m_FeatureDetector == null)
                {
                    points = null;
                    return false;
                }
            }

            // Return array of points no matter which mode it supports so other systems can rely on only one entry point of API
            if (m_FeatureDetector.SupportsMultipleMatches)
                points = m_FeatureDetector.FindMultipleTextPositions(detectable.Value as string, observedImage).ToArray();
            else
                points = new[] { m_FeatureDetector.FindTextPosition(detectable.Value as string, observedImage) };

            return points != null && points.Length > 0 && points[0] != null;
        }
    }
}
