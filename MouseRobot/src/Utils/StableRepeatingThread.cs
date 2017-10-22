using System;
using System.Diagnostics;
using System.Threading;

namespace Robot.Utils
{
    public class StableRepeatingThread
    {
        private Thread m_Thread;
        private Action m_Action;
        private bool m_Run;

        public StableRepeatingThread(Action action, int FPS = 60)
        {
            this.FPS = FPS;
            m_Thread = new Thread(new ThreadStart(ThreadRun));
            m_Action = action;
            m_Stopwatch = new Stopwatch();
        }

        // Thread methods / properties
        public void Start() { m_Run = true; m_Thread.Start(); }
        public void Join() { m_Thread.Join(); }
        public bool IsAlive { get { return m_Thread.IsAlive; } }
        public void Stop() { m_Run = false; }

        // Stable thread properties
        private int m_FPS;
        public int FPS
        {
            get
            {
                return m_FPS;
            }
            set
            {
                m_FPS = value;
                m_FrameTimeMax = 1000 / m_FPS;
            }
        }

        private int m_FrameTimeMax;
        public int FrameTimeMax
        {
            get
            {
                return m_FrameTimeMax;
            }
            set
            {
                m_FrameTimeMax = value;
                m_FPS = 1000 / m_FrameTimeMax;
            }
        }

        private Stopwatch m_Stopwatch;

        private void ThreadRun()
        {
            while (m_Run)
            {
                m_Stopwatch.Start();
                m_Action.Invoke();
                m_Stopwatch.Stop();

                var elapsed = (int)m_Stopwatch.ElapsedMilliseconds;
                if (elapsed < FrameTimeMax)
                    Thread.Sleep(FrameTimeMax - elapsed);

                m_Stopwatch.Reset();
                Debug.WriteLine(string.Format("Frame took {0} millis to complete", elapsed));
            }
        }
    }
}
