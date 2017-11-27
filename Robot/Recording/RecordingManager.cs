using Robot.Scripts;
using Robot.Settings;
using Robot.Utils.Win32;
using RobotRuntime.Commands;
using RobotRuntime.Graphics;
using RobotRuntime.Perf;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Robot.Recording
{
    public class RecordingManager
    {
        private Stopwatch m_SleepTimer = new Stopwatch();
        private Point m_LastClickPos = new Point(0, 0);

        private Point m_LastImagePosition = Point.Empty;
        private bool m_ActionOnImage = false;

        public event Action<Asset, Point> ImageFoundInAssets;
        public event Action<Point> ImageNotFoundInAssets;

        private readonly Size k_ImageForReferenceSearchUnderCursorSize = new Size(12, 12);

        static private RecordingManager m_Instance = new RecordingManager();
        static public RecordingManager Instance { get { return m_Instance; } }
        private RecordingManager()
        {
            InputCallbacks.inputEvent += OnInputEvent;
        }

        private void OnInputEvent(KeyEvent e)
        {
            if (!MouseRobot.Instance.IsRecording)
                return;

            if (ScriptManager.Instance.LoadedScripts.Count == 0)
                return;



            var activeScript = ScriptManager.Instance.ActiveScript;
            var props = SettingsManager.Instance.RecordingSettings;

            if (!CroppingManager.Instance.IsCropping)
            {
                if (ShouldStartCropImage(e, props))
                    return;
            }
            else
            {
                if (ShouldEndCropImage(e, props));
                    return;
            }

            RecordCommand(e, activeScript, props);
            ImageFindOperations(e, activeScript, props);
        }

        private void RecordCommand(KeyEvent e, Script activeScript, RecordingSettings props)
        {
            var imageSmooth = true;
            var timeOut = 2000;

            if (e.IsKeyDown())
            {
                if (e.keyCode == props.DefaultSleepKey)
                    activeScript.AddCommand(new CommandSleep(props.DefaultSleepTime));

                if (e.keyCode == props.SleepKey)
                    m_SleepTimer.Restart();

                if (e.keyCode == props.SmoothMouseMoveKey)
                {
                    activeScript.AddCommand(new CommandMove(e.X, e.Y));
                    m_LastClickPos = e.Point;
                    // TODO: TIME ?
                }

                if (e.keyCode == props.MouseDownButton)
                {
                    if (!m_ActionOnImage && props.AutomaticSmoothMoveBeforeMouseDown && Distance(m_LastClickPos, e.Point) > 20)
                        activeScript.AddCommand(new CommandMove(e.X, e.Y)); // TODO: TIME ?

                    if (props.TreatMouseDownAsMouseClick)
                        if (m_ActionOnImage)
                        {
                            AddMoveToImageOrNoneIfAlreadyOnImage(e, activeScript, props, timeOut);
                            activeScript.AddCommand(new CommandPress(e.X, e.Y, true));
                        }
                        else
                            activeScript.AddCommand(new CommandPress(e.X, e.Y, false));

                    else if (m_ActionOnImage)
                    {
                        AddMoveToImageOrNoneIfAlreadyOnImage(e, activeScript, props, timeOut);
                        activeScript.AddCommand(new CommandDown(e.X, e.Y, true));
                    }
                    else
                        activeScript.AddCommand(new CommandDown(e.X, e.Y, false));

                    m_LastClickPos = e.Point;
                }
            }
            else if (e.IsKeyUp())
            {
                if (e.keyCode == props.SleepKey)
                {
                    m_SleepTimer.Stop();
                    activeScript.AddCommand(new CommandSleep((int)m_SleepTimer.ElapsedMilliseconds));
                }

                if (e.keyCode == props.MouseDownButton && !props.TreatMouseDownAsMouseClick)
                {
                    if (!m_ActionOnImage && props.AutomaticSmoothMoveBeforeMouseUp && Distance(m_LastClickPos, e.Point) > 20)
                        activeScript.AddCommand(new CommandMove(e.X, e.Y)); // TODO: TIME ?

                    else if (!m_ActionOnImage)
                    {
                        if (props.ThresholdBetweenMouseDownAndMouseUp > 0)
                            activeScript.AddCommand(new CommandSleep(props.ThresholdBetweenMouseDownAndMouseUp));
                    }

                    if (m_ActionOnImage)
                    {
                        AddMoveToImageOrNoneIfAlreadyOnImage(e, activeScript, props, timeOut);
                        activeScript.AddCommand(new CommandRelease(e.X, e.Y, true));
                    }
                    else
                        activeScript.AddCommand(new CommandRelease(e.X, e.Y, false));

                    m_LastClickPos = e.Point;
                }
            }
            else
            {
                throw new Exception("Key event was fired but neither KeyUp or KeyDown was true");
            }
        }

        private void AddMoveToImageOrNoneIfAlreadyOnImage(KeyEvent e, Script activeScript, RecordingSettings props, int timeOut)
        {
            if (Distance(m_LastImagePosition, e.Point) > 40)
            {
                var imageAsset = FindImage(e.Point);
                if (imageAsset != null)
                    activeScript.AddCommand(new CommandMoveOnImage(imageAsset.ToAssetPointer(), timeOut, props.AutomaticSmoothMoveBeforeMouseDown));
            }
        }

        private void ImageFindOperations(KeyEvent e, Script activeScript, RecordingSettings props)
        {
            if (e.IsKeyDown())
            {
                if (e.keyCode == props.PerformActionOnImage)
                    m_ActionOnImage = true;

                if (e.keyCode == props.FindImage)
                    FindImage(e.Point);
            }
            else if (e.IsKeyUp())
            {
                if (e.keyCode == props.PerformActionOnImage)
                    m_ActionOnImage = false;
            }
        }

        private Asset FindImage(Point cursorPos)
        {
            Profiler.Start("RecordingManager_FindImage");
            Profiler.Start("RecordingManager_CropFromScreen");

            var size = k_ImageForReferenceSearchUnderCursorSize;
            var crop = BitmapUtility.TakeScreenshotOfSpecificRect(
                WinAPI.GetCursorPosition().Sub(new Point(size.Width / 2, size.Width / 2)), size);

            Profiler.Stop("RecordingManager_CropFromScreen");
            Profiler.Start("RecordingManager_FindAssetReference");

            // TODO: FeatureDetector.Get will fail to find proper match if another thread already used/is using it
            var detector = FeatureDetector.Create(DetectionMode.PixelPerfect);
            Asset retAsset = null;
            foreach (var asset in AssetManager.Instance.Assets)
            {
                if (asset.HoldsTypeOf(typeof(Bitmap)))
                {
                    if (detector.FindImagePos(crop, asset.Importer.Load<Bitmap>()) != null)
                    {
                        ImageFoundInAssets?.Invoke(asset, cursorPos);
                        retAsset = asset;
                        break;
                    }
                }
            }

            Profiler.Stop("RecordingManager_FindAssetReference");
            Profiler.Stop("RecordingManager_FindImage");

            if (retAsset == null)
                ImageNotFoundInAssets?.Invoke(cursorPos);

            return retAsset;
        }

        private bool ShouldStartCropImage(KeyEvent e, RecordingSettings props)
        {
            if (e.IsKeyDown() && e.keyCode == props.CropImage)
            {
                CroppingManager.Instance.StartCropImage(e.Point);
                return true;
            }
            return false;
        }

        private bool ShouldEndCropImage(KeyEvent e, RecordingSettings props)
        {
            if (e.IsKeyDown())
            {
                if (e.keyCode == props.CropImage)
                    CroppingManager.Instance.EndCropImage(e.Point);
                else
                    CroppingManager.Instance.CancelCropImage();

                return true;
            }
            return false;
        }

        private float Distance(Point a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
