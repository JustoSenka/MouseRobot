using RobotRuntime.Commands;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public class ScriptThread
    {
        static private ScriptThread m_Instance = new ScriptThread();
        static public ScriptThread Instance { get { return m_Instance; } }
        private ScriptThread() { }

        public event Action Finished;

        private bool m_Run;

        public void Start(LightScript lightScript)
        {
            m_Run = true;

            //RuntimeSettings.ApplySettings();
            ScreenStateThread.Instace.Start();
            FeatureDetectionThread.Instace.Start();
            Task.Delay(80).Wait(); // make sure first screenshot is taken before starting running commands

            new Thread(delegate ()
            {
                Console.WriteLine("Script start");
                foreach (var v in lightScript.Commands)
                {
                    RunCommand(v);

                    if (!m_Run)
                        break;
                }

                ScreenStateThread.Instace.Stop();
                FeatureDetectionThread.Instace.Stop();

                Finished?.Invoke();
                Console.WriteLine("End script.");
            }).Start();
        }

        private static void RunCommand(TreeNode<Command> node)
        {
            Console.WriteLine(node.value.ToString());

            bool isImageCommand = node.value.CommandType == CommandType.ForeachImage || node.value.CommandType == CommandType.ForImage;

            if (!isImageCommand)
            {
                node.value.Run();
                return;
            }
            else
            {
                AssetPointer image;
                int timeout;
                GetImageAndTimeout(node, out image, out timeout);

                var points = GetCoordinates(node, image, timeout);
                if (points == null || points.Length == 0)
                    return;

                foreach (var p in points)
                {
                    node.value.Run();
                    foreach (var childNode in node)
                    {
                        OverrideCommandPropertiesIfExist(childNode.value, p.X, "X");
                        OverrideCommandPropertiesIfExist(childNode.value, p.Y, "Y");
                        childNode.value.Run();
                    }
                }
            }
        }

        private static Point[] GetCoordinates(TreeNode<Command> node, AssetPointer image, int timeout)
        {
            var command = node.value;

            if (node.value.CommandType != CommandType.ForeachImage && node.value.CommandType != CommandType.ForImage)
                return null;

            int x1 = WinAPI.GetCursorPosition().X;
            int y1 = WinAPI.GetCursorPosition().Y;

            FeatureDetectionThread.Instace.StartNewImageSearch(image);
            while (timeout > FeatureDetectionThread.Instace.TimeSinceLastFind)
            {
                Task.Delay(5).Wait(); // It will probably wait 15-30 ms, depending on thread clock, find better solution
                if (FeatureDetectionThread.Instace.WasImageFound)
                    break;
            }

            if (FeatureDetectionThread.Instace.WasImageFound)
                return FeatureDetectionThread.Instace.LastKnownPositions.Select(p => p.FindCenter()).ToArray();
            else
                return null;
        }

        private static void GetImageAndTimeout(TreeNode<Command> node, out AssetPointer image, out int timeout)
        {
            var command = node.value;
            if (command is CommandForImage)
            {
                var c = (CommandForImage)command;
                image = c.Asset;
                timeout = c.Timeout;
            }
            else if (command is CommandForeachImage)
            {
                var c = (CommandForeachImage)command;
                image = c.Asset;
                timeout = c.Timeout;
            }
            else
            {
                image = default(AssetPointer);
                timeout = 0;
            }
        }

        private static void OverrideCommandPropertiesIfExist(Command command, object value, string prop)
        {
            var destProp = command.GetType().GetProperty(prop);

            if (destProp != null)
                destProp.SetValue(command, value);
        }

        public void Stop()
        {
            m_Run = false;
        }
    }
}
