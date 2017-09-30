using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Robot.Utils.Win32;
using System.Windows.Forms;

namespace Robot
{
    public class MouseRobot
    {
        static private MouseRobot m_Instance = new MouseRobot();
        static public MouseRobot Instance { get { return m_Instance; } }

        public event Action<bool> RecordingStateChanged;
        public event Action<bool> PlayingStateChanged;

        private bool m_IsRecording;
        private bool m_IsPlaying;

        public CommandManagerProperties commandManagerProperties;

        private MouseRobot()
        {
            ScriptManager.Instance.NewScript();
            InputCallbacks.inputEvent += OnInputEvent;
        }

        public void ForceInit() { } // This is to make sure that mouse robot singleton is created

        private void OnInputEvent(KeyEvent obj)
        {
            if (!m_IsRecording)
                return;

            if (ScriptManager.Instance.LoadedScripts.Count == 0)
                return;

            if (obj.IsKeyDown())
            {
                if (obj.keyCode == commandManagerProperties.SleepKey)
                    ScriptManager.Instance.ActiveScript.AddCommandSleep(commandManagerProperties.DefaultSleepTime);

            } // IsKeyDown = true
            else if (obj.IsKeyUp())
            {

            } // IsKeyUp = true
            else
            {
                throw new Exception("Key event was fired but neither KeyUp or KeyDown was true");
            }

        }

        public void StartScript(int repeatTimes)
        {
            if (ScriptManager.Instance.ActiveScript == null)
                return;

            ScriptThread.Instance.Start(ScriptManager.Instance.ActiveScript, repeatTimes);
        }

        public void StopScript()
        {
            // TODO:
        }

        public void AddCommandSleep(int time)
        {
            ScriptManager.Instance.ActiveScript.AddCommandSleep(time);
        }

        public void AddCommandRelease()
        {
            ScriptManager.Instance.ActiveScript.AddCommandRelease();
        }

        public void AddCommandPress(int x, int y)
        {
            ScriptManager.Instance.ActiveScript.AddCommandPress(x, y);
        }

        public void AddCommandMove(int x, int y)
        {
            ScriptManager.Instance.ActiveScript.AddCommandMove(x, y);
        }

        public void AddCommandDown(int x, int y)
        {
            ScriptManager.Instance.ActiveScript.AddCommandDown(x, y);
        }

        public bool IsRecording
        {
            get
            {
                return m_IsRecording;
            }
            set
            {
                if (m_IsPlaying)
                    throw new InvalidOperationException("Cannot record while playing script.");

                if (value != m_IsRecording)
                    RecordingStateChanged?.Invoke(value);

                m_IsRecording = value;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return m_IsPlaying;
            }
            set
            {
                if (m_IsRecording)
                    throw new InvalidOperationException("Cannot play a script while is recording.");

                if (value != m_IsPlaying)
                    PlayingStateChanged?.Invoke(value);

                m_IsPlaying = value;
            }
        }
    }
}
