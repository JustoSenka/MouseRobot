using RobotEditor.Abstractions;
using RobotEditor.Windows.Base;
using System;
using System.Windows.Forms;
using Unity;
using RobotRuntime;
using System.Linq;
using System.Collections.Generic;

namespace RobotEditor.Windows
{
    public class ScreenPaintForm : InvisibleForm, IScreenPaintForm
    {
        private new IUnityContainer Container;
        private IPaintOnScreen[] m_Painters;
        private IList<IPaintOnScreen> m_RegisteredPainters = new List<IPaintOnScreen>();

        public ScreenPaintForm(IUnityContainer Container) : base()
        {
            this.Container = Container;

            SubscribeToAllPainters();

            m_UpdateTimer.Interval = 30;
            m_UpdateTimer.Tick += CallInvalidate;
            m_UpdateTimer.Enabled = true;
        }

        public void SubscribeToAllPainters()
        {
            if (m_Painters != null)
            {
                foreach (var p in m_Painters)
                {
                    p.StartInvalidateOnTimer -= AddClassToInvalidateList;
                    p.StopInvalidateOnTimer -= RemoveClassFromInvalidateList;
                    p.Invalidate -= Invalidate;
                }
            }

            var types = AppDomain.CurrentDomain.GetAllTypesWhichImplementInterface(typeof(IPaintOnScreen));
            m_Painters = types.Select(t => Container.Resolve(t)).Cast<IPaintOnScreen>().ToArray(); // TODO: Add catch if resolve fails

            if (m_Painters != null)
            {
                foreach (var p in m_Painters)
                {
                    p.StartInvalidateOnTimer += AddClassToInvalidateList;
                    p.StopInvalidateOnTimer += RemoveClassFromInvalidateList;
                    p.Invalidate += Invalidate;
                }
            }
        }

        public void AddClassToInvalidateList(IPaintOnScreen instance)
        {
            if (!m_RegisteredPainters.Contains(instance))
                m_RegisteredPainters.Add(instance);

            Invalidate();
        }

        public void RemoveClassFromInvalidateList(IPaintOnScreen instance)
        {
            if (m_RegisteredPainters.Contains(instance))
                m_RegisteredPainters.Remove(instance);

            Invalidate();
        }

        private void CallInvalidate(object sender, EventArgs e)
        {
            if (m_RegisteredPainters.Count > 0)
                Invalidate();
        }

        private Timer m_UpdateTimer = new Timer();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            foreach (var p in m_Painters)
                p.OnPaint(e);
        }
    }
}
