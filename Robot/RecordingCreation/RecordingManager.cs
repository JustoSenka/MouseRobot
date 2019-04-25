using Robot.Abstractions;
using Robot.Settings;
using Robot.Utils.Win32;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Graphics;
using RobotRuntime.Recordings;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using Unity.Lifetime;

namespace Robot.RecordingCreation
{
    [RegisterTypeToContainer(typeof(IRecordingManager), typeof(ContainerControlledLifetimeManager))]
    public class RecordingManager : IRecordingManager
    {
        public bool IsRecording { get; set; }

        private Stopwatch m_SleepTimer = new Stopwatch();
        private Point m_LastClickPos = new Point(0, 0);

        private Asset m_ImageAssetUnderCursor;
        private Command m_ParentCommand;
        private bool m_ForImage = false;
        private bool m_ForEachImage = false;

        public event Action<Asset, Point> ImageFoundInAssets;
        public event Action<Point> ImageNotFoundInAssets;

        private readonly Size k_ImageForReferenceSearchUnderCursorSize = new Size(12, 12);

        private IHierarchyManager HierarchyManager;
        private ISettingsManager SettingsManager;
        private ICroppingManager CroppingManager;
        private IAssetManager AssetManager;
        private IProfiler Profiler;
        private IFeatureDetectorFactory FeatureDetectorFactory;
        private IInputCallbacks InputCallbacks;
        public RecordingManager(IHierarchyManager HierarchyManager, ISettingsManager SettingsManager, ICroppingManager CroppingManager, IAssetManager AssetManager, IProfiler Profiler,
            IFeatureDetectorFactory FeatureDetectorFactory, IInputCallbacks InputCallbacks)
        {
            this.HierarchyManager = HierarchyManager;
            this.SettingsManager = SettingsManager;
            this.CroppingManager = CroppingManager;
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;
            this.FeatureDetectorFactory = FeatureDetectorFactory;
            this.InputCallbacks = InputCallbacks;

            InputCallbacks.InputEvent += OnInputEvent;
        }

        private void OnInputEvent(KeyEvent e)
        {
            if (!IsRecording)
                return;

            if (HierarchyManager.LoadedRecordings.Count == 0)
                return;

            var activeRecording = HierarchyManager.ActiveRecording;
            var props = SettingsManager.GetSettings<RecordingSettings>();

            if (!CroppingManager.IsCropping)
            {
                if (ShouldStartCropImage(e, props))
                    return;
            }
            else
            {
                if (ShouldEndCropImage(e, props))
                    return;
            }

            RecordCommand(e, activeRecording, props);
            ImageFindOperations(e, activeRecording, props);
        }

        private void RecordCommand(KeyEvent e, Recording activeRecording, RecordingSettings props)
        {
            var isMouseButtonUsed = e.keyCode == props.LeftMouseDownButton ||
                e.keyCode == props.RightMouseDownButton ||
                e.keyCode == props.MiddleMouseDownButton;

            var mouseButton = e.keyCode == props.LeftMouseDownButton ? MouseButton.Left :
                e.keyCode == props.RightMouseDownButton ? MouseButton.Right :
                MouseButton.Middle;

            if (e.IsKeyDown())
            {
                if (e.keyCode == props.DefaultSleepKey)
                    AddCommand(new CommandSleep(props.DefaultSleepTime));

                if (e.keyCode == props.SleepKey)
                    m_SleepTimer.Restart();

                if (e.keyCode == props.SmoothMouseMoveKey)
                {
                    AddCommand(new CommandMove(e.X, e.Y));
                    m_LastClickPos = e.Point;
                    // TODO: TIME ?
                }

                if (isMouseButtonUsed)
                {
                    if (!m_ForImage && props.AutomaticSmoothMoveBeforeMouseDown && Distance(m_LastClickPos, e.Point) > 20)
                        AddCommand(new CommandMove(e.X, e.Y)); // TODO: TIME ?

                    if (props.TreatMouseDownAsMouseClick)
                        AddCommand(new CommandPress(e.X, e.Y, false, mouseButton));
                    else
                        AddCommand(new CommandDown(e.X, e.Y, false, mouseButton));

                    m_LastClickPos = e.Point;
                }
            }
            else if (e.IsKeyUp())
            {
                if (e.keyCode == props.SleepKey)
                {
                    m_SleepTimer.Stop();
                    AddCommand(new CommandSleep((int)m_SleepTimer.ElapsedMilliseconds));
                }

                if (isMouseButtonUsed && !props.TreatMouseDownAsMouseClick)
                {
                    if (props.AutomaticSmoothMoveBeforeMouseUp && Distance(m_LastClickPos, e.Point) > 20)
                        AddCommand(new CommandMove(e.X, e.Y)); // TODO: TIME ?

                    if (props.ThresholdBetweenMouseDownAndMouseUp > 0)
                        AddCommand(new CommandSleep(props.ThresholdBetweenMouseDownAndMouseUp));

                    AddCommand(new CommandRelease(e.X, e.Y, false));

                    m_LastClickPos = e.Point;
                }
            }
            else
            {
                Logger.Log(LogType.Warning, "Key event was fired but neither KeyUp or KeyDown was true");
            }
        }

        private void AddCommand(Command command)
        {
            if (m_ForImage || m_ForEachImage)
                HierarchyManager.ActiveRecording.AddCommand(command, m_ParentCommand);
            else
                HierarchyManager.ActiveRecording.AddCommand(command);
        }

        private void ImageFindOperations(KeyEvent e, Recording activeRecording, RecordingSettings props)
        {
            var timeOut = 2000;

            if (e.IsKeyDown())
            {
                if (e.keyCode == props.ForEachImage && m_ForImage == false)
                {
                    m_ImageAssetUnderCursor = FindImage(e.Point);
                    if (m_ImageAssetUnderCursor != null)
                    {
                        m_ParentCommand = new CommandForImage(m_ImageAssetUnderCursor.Guid, timeOut, true);
                        HierarchyManager.ActiveRecording.AddCommand(m_ParentCommand);
                        m_ForImage = true;
                    }
                }

                if (e.keyCode == props.ForImage && m_ForEachImage == false)
                {
                    m_ImageAssetUnderCursor = FindImage(e.Point);
                    if (m_ImageAssetUnderCursor != null)
                    {
                        m_ParentCommand = new CommandForImage(m_ImageAssetUnderCursor.Guid, timeOut, false);
                        HierarchyManager.ActiveRecording.AddCommand(m_ParentCommand);
                        m_ForEachImage = true;
                    }
                }

                if (e.keyCode == props.FindImage)
                    FindImage(e.Point);
            }
            else if (e.IsKeyUp())
            {
                if (e.keyCode == props.ForEachImage)
                    m_ForImage = false;

                if (e.keyCode == props.ForImage)
                    m_ForEachImage = false;
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
            var detector = FeatureDetectorFactory.Create(DetectorNamesHardcoded.PixelPerfect);
            Asset retAsset = null;
            foreach (var asset in AssetManager.Assets)
            {
                if (asset.HoldsTypeOf(typeof(Bitmap)))
                {
                    var bmp = asset.Importer.Load<Bitmap>();
                    if (bmp == null)
                        continue;

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
                CroppingManager.StartCropImage(e.Point);
                return true;
            }
            return false;
        }

        private bool ShouldEndCropImage(KeyEvent e, RecordingSettings props)
        {
            if (e.IsKeyDown())
            {
                if (e.keyCode == props.CropImage)
                    CroppingManager.EndCropImage(e.Point);
                else
                    CroppingManager.CancelCropImage();

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
