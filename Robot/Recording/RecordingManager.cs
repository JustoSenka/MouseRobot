using Robot.Scripts;
using Robot.Settings;
using Robot.Utils.Win32;
using RobotRuntime;
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

        private Asset m_ImageAssetUnderCursor;
        private Command m_ParentCommand;
        private bool m_ForImage = false;
        private bool m_ForEachImage = false;

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
                if (ShouldEndCropImage(e, props)) ;
                return;
            }

            RecordCommand(e, activeScript, props);
            ImageFindOperations(e, activeScript, props);
        }

        private void RecordCommand(KeyEvent e, Script activeScript, RecordingSettings props)
        {
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

                if (e.keyCode == props.MouseDownButton)
                {
                    if (!m_ForImage && props.AutomaticSmoothMoveBeforeMouseDown && Distance(m_LastClickPos, e.Point) > 20)
                        AddCommand(new CommandMove(e.X, e.Y)); // TODO: TIME ?

                    if (props.TreatMouseDownAsMouseClick)
                        AddCommand(new CommandPress(e.X, e.Y, false));
                    else
                        AddCommand(new CommandDown(e.X, e.Y, false));

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

                if (e.keyCode == props.MouseDownButton && !props.TreatMouseDownAsMouseClick)
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
                throw new Exception("Key event was fired but neither KeyUp or KeyDown was true");
            }
        }

        private void AddCommand(Command command)
        {
            if (m_ForImage || m_ForEachImage)
                ScriptManager.Instance.ActiveScript.AddCommand(command, m_ParentCommand);
            else
                ScriptManager.Instance.ActiveScript.AddCommand(command);
        }

        private void ImageFindOperations(KeyEvent e, Script activeScript, RecordingSettings props)
        {
            var timeOut = 2000;

            if (e.IsKeyDown())
            {
                if (e.keyCode == props.ForEachImage && m_ForImage == false)
                {
                    m_ImageAssetUnderCursor = FindImage(e.Point);
                    if (m_ImageAssetUnderCursor != null)
                    {
                        m_ParentCommand = new CommandForeachImage(m_ImageAssetUnderCursor.ToAssetPointer(), timeOut);
                        ScriptManager.Instance.ActiveScript.AddCommand(m_ParentCommand);
                        m_ForImage = true;
                    }
                }

                if (e.keyCode == props.ForImage && m_ForEachImage == false)
                {
                    m_ImageAssetUnderCursor = FindImage(e.Point);
                    if (m_ImageAssetUnderCursor != null)
                    {
                        m_ParentCommand = new CommandForImage(m_ImageAssetUnderCursor.ToAssetPointer(), timeOut);
                        ScriptManager.Instance.ActiveScript.AddCommand(m_ParentCommand);
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
