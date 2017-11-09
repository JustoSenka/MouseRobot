using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RobotRuntime.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RobotRuntime.Graphics
{
    public abstract class FeatureDetector
    {
        public abstract Point[] FindImagePos(Bitmap sampleImage, Bitmap observedImage);
        public abstract IEnumerable<Point[]> FindImageMultiplePos(Bitmap sampleImage, Bitmap observedImage);

        protected virtual int MinimumImageScaleSize { get { return 100; } }
        protected virtual float MaxScaleDownFactor { get { return 0.25f; } }
        protected virtual float MaxScaleUpFactor { get { return 2f; } }
        public virtual bool SupportsMultipleMatches { get { return false; } }

        protected float SmartResize(ref Image<Bgr, byte> big, ref Image<Bgr, byte> small, bool canChangeTheRatio = false)
        {
            var minS = small.Width < small.Height ? small.Width : small.Height;
            var minB = big.Width < big.Height ? big.Width : big.Height;

            float scaleS = minS > MinimumImageScaleSize ? MinimumImageScaleSize * 1.0f / minS : 1;
            scaleS = scaleS.Clamp(MaxScaleDownFactor, MaxScaleUpFactor);

            float scaleB = minB > MinimumImageScaleSize ? MinimumImageScaleSize * 1.0f / minB : 1;
            scaleB = scaleB.Clamp(MaxScaleDownFactor, MaxScaleUpFactor);

            if (canChangeTheRatio)
            {
                big = big.Resize(scaleS, Inter.Linear);
                small = small.Resize(scaleB, Inter.Linear);
            }
            else
            {
                scaleB = scaleS > scaleB ? scaleS : scaleB;
                big = big.Resize(scaleB, Inter.Linear);
                small = small.Resize(scaleB, Inter.Linear);
            }

            return scaleB;
        }


        private static FeatureDetector s_CurrentDetector;
        private static DetectionMode s_CurrentDetectionMode;
        public static FeatureDetector Get(DetectionMode detectionMode)
        {
            if (s_CurrentDetector != null && detectionMode == s_CurrentDetectionMode)
                return s_CurrentDetector;
            else
                return s_CurrentDetector = Create(detectionMode);
        }

        private static FeatureDetector Create(DetectionMode detectionMode)
        {
            switch (detectionMode)
            {
                case DetectionMode.FeatureSURF:
                    return new FeatureDetectorSURF();
                case DetectionMode.PixelPerfect:
                    return new FeatureDetectorPP();
                case DetectionMode.Template:
                    return new FeatureDetectorTemplate();
                default:
                    return null;
            }
        }
    }
}
