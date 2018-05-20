using RobotEditor.Abstractions;
using RobotEditor.Windows.Base;
using System;
using System.Windows.Forms;
using Unity;
using RobotRuntime;
using System.Linq;
using System.Collections.Generic;
using RobotRuntime.Abstractions;

namespace RobotEditor.Windows
{
    public class ScreenPaintForm : InvisibleForm, IScreenPaintForm
    {
        private IPaintOnScreen[] m_NativePainters;
        private IPaintOnScreen[] m_UserPainters;
        private IPaintOnScreen[] m_Painters;

        private IList<IPaintOnScreen> m_RegisteredPainters = new List<IPaintOnScreen>();

        private new IUnityContainer Container;
        private IPluginLoader PluginLoader;
        private ILogger Logger;
        public ScreenPaintForm(IUnityContainer Container, IPluginLoader PluginLoader, ILogger Logger) : base()
        {
            this.Container = Container;
            this.PluginLoader = PluginLoader;
            this.Logger = Logger;

            CollectNativePainters();
            CollectUserPainters();
            SubscribeToAllPainters();

            PluginLoader.UserDomainReloaded += OnDomainReloaded;

            m_UpdateTimer.Interval = 30;
            m_UpdateTimer.Tick += CallInvalidate;
            m_UpdateTimer.Enabled = true;
        }

        private void CollectNativePainters()
        {
            var types = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(IPaintOnScreen));
            m_NativePainters = types.Select(t => Container.Resolve(t)).Cast<IPaintOnScreen>().ToArray();
        }

        private void CollectUserPainters()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            var types = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(IPaintOnScreen));
            m_UserPainters = types.TryResolveTypes(Container, Logger).Cast<IPaintOnScreen>().ToArray();
        }

        public void SubscribeToAllPainters()
        {
            var newPainters = m_NativePainters.Concat(m_UserPainters).ToArray();

            if (m_Painters != null)
            {
                foreach (var p in m_Painters)
                {
                    p.StartInvalidateOnTimer -= AddClassToInvalidateList;
                    p.StopInvalidateOnTimer -= RemoveClassFromInvalidateList;
                    p.Invalidate -= Invalidate;

                    if (!newPainters.Contains(p) && m_RegisteredPainters.Contains(p))
                        m_RegisteredPainters.Remove(p);
                }
            }

            m_Painters = newPainters;

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

        private void OnDomainReloaded()
        {
            CollectUserPainters();
            SubscribeToAllPainters();
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

            foreach (var p in m_Painters)
                p.OnPaint(e);
        }
    }
}
