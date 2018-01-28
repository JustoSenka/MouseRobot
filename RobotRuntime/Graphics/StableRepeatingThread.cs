using RobotRuntime.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;

namespace RobotRuntime.Graphics
{
    public abstract class StableRepeatingThread : IStableRepeatingThread
    {
        public event Action Update;

        private Thread m_Thread;
        private Action m_Action;
        private bool m_Run;
        private bool m_Init;

        protected virtual string Name { get { return "StableRepeatingThread"; } }

        protected StableRepeatingThread()
        {
            m_Thread = new Thread(new ThreadStart(ThreadRun));
            m_Stopwatch = new Stopwatch();
        }

        // Thread methods / properties
        public virtual void Init() { m_Init = true; }
        public void Start(int FPS = 0)
        {
            if (!m_Init)
                Init();

            this.FPS = FPS == 0 && m_FPS == 0 ? 30 : (m_FPS == 0) ? FPS : 30;
            m_Run = true;

            if (!m_Thread.IsAlive)
                m_Thread = new Thread(new ThreadStart(ThreadRun));

            m_Thread.Start();
        }

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
                ThreadAction();
                m_Stopwatch.Stop();

                var elapsed = (int)m_Stopwatch.ElapsedMilliseconds;
                if (elapsed < FrameTimeMax)
                    Thread.Sleep(FrameTimeMax - elapsed);

                m_Stopwatch.Reset();

                //System.Diagnostics.Debug.WriteLine(Name + " took " + elapsed + " ms. to complete");
                Update?.Invoke();
            }
        }

        protected abstract void ThreadAction();
    }
}
