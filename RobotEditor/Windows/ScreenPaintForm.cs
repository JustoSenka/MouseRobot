using RobotEditor.Abstractions;
using RobotEditor.Windows.Base;
using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RobotEditor.Windows
{
    public class ScreenPaintForm : InvisibleForm, IScreenPaintForm
    {
        private IList<IPaintOnScreen> m_RegisteredPainters = new List<IPaintOnScreen>();

        private IEnumerable<IPaintOnScreen> m_OldPainters;

        private ITypeObjectCollector<IPaintOnScreen> TypeCollector;
        public ScreenPaintForm(ITypeObjectCollector<IPaintOnScreen> TypeCollector) : base()
        {
            this.TypeCollector = TypeCollector;

            TypeCollector.NewTypesAppeared += SubscribeToAllPainters;
            SubscribeToAllPainters();

            m_UpdateTimer.Interval = 30;
            m_UpdateTimer.Tick += CallInvalidate;
            m_UpdateTimer.Enabled = true;
        }

        public void SubscribeToAllPainters()
        {
            if (m_OldPainters != null)
            {
                foreach (var p in m_OldPainters)
                {
                    p.StartInvalidateOnTimer -= AddClassToInvalidateList;
                    p.StopInvalidateOnTimer -= RemoveClassFromInvalidateList;
                    p.Invalidate -= Invalidate;

                    if (!TypeCollector.AllObjects.Contains(p) && m_RegisteredPainters.Contains(p))
                        m_RegisteredPainters.Remove(p);
                }
            }

            m_OldPainters = TypeCollector.AllObjects;

            if (m_OldPainters != null)
            {
                foreach (var p in m_OldPainters)
                {
                    p.StartInvalidateOnTimer += AddClassToInvalidateList;
                    p.StopInvalidateOnTimer += RemoveClassFromInvalidateList;
                    p.Invalidate += Invalidate;
                }
            }

            Invalidate();
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

            foreach (var p in TypeCollector.AllObjects)
                p.OnPaint(e);
        }
    }
}
