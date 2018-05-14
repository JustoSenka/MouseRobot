using RobotEditor.Abstractions;
using RobotEditor.Windows.Base;
using System;
using System.Windows.Forms;
using Unity;
using RobotRuntime;
using System.Linq;
using System.Collections.Generic;
using RobotRuntime.Abstractions;
using RobotRuntime.Plugins;
using RobotRuntime.Utils;
using System.Drawing;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace RobotEditor.Windows
{
    public class ScreenPaintForm : InvisibleForm, IScreenPaintForm
    {
        private List<IPaintOnScreen> m_Painters = new List<IPaintOnScreen>();
        private IList<IPaintOnScreen> m_RegisteredPainters = new List<IPaintOnScreen>();

        private new IUnityContainer Container;
        private IPluginLoader PluginLoader;
        public ScreenPaintForm(IUnityContainer Container, IPluginLoader PluginLoader) : base()
        {
            this.Container = Container;
            this.PluginLoader = PluginLoader;

            SubscribeToAllPainters();

            PluginLoader.UserDomainReloaded += OnDomainReloaded;

            m_UpdateTimer.Interval = 30;
            m_UpdateTimer.Tick += CallInvalidate;
            m_UpdateTimer.Enabled = true;

            OnDomainReloaded(); // TODO: Shouldn't OnDomainReloaded be called from PluginDomainManager on startup instead?
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
            m_Painters = types.Select(t => Container.Resolve(t)).Cast<IPaintOnScreen>().ToList(); // TODO: Add catch if resolve fails

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
            var typeNames = PluginLoader.PluginDomainManager.GetAllTypeNamesWhichImplementInterface(typeof(ITest));

            var o = PluginLoader.PluginDomainManager.Instantiate(typeNames[0]);



            /* var handle = (ObjectHandle) o;
             var proxy = handle.Unwrap();

             var instance = (ITest) proxy;*/


            /**
             *  It is possible to instantiate user created classes and return them here and use them.
             *  NOT possible to pass non serializable data to them (like Container or PaintEventArgs)
             *  WIll be difficult to pass all managers since all the data must be serialized.
             *  Managers will probaby lose their reference connection.
             * 
             *  Not sure how to achieve..
             * 
             * */

            var instance = (ITest)o;
            var val = instance.Pow(new ValueWrapper(0));

            Logger.Log(LogType.Log, "Val: " + val.Value);

            /**
             * Instantiate using activator on the domain itself. Creates fine, but fails to pass PaintEventArgs due to serialization
            
            var typeNames = PluginLoader.PluginDomainManager.GetAllTypeNamesWhichImplementInterface(typeof(IPaintOnScreen)); 

            var o = PluginLoader.PluginDomainManager.Instantiate(typeNames[0]);
            var instance = o as IPaintOnScreen;

            var painterInstances = new[] { instance }; */


            /**
             *  Try to get type and instantiate with container : TypeLoadException
             
            var typeWrapper = PluginLoader.PluginDomainManager.GetAllTypesWhichImplementInterface(typeof(IPaintOnScreen));
            
            var types = typeWrapper.TypeList;
            var instances = types.Select(t => Container.Resolve(t));

            var painterInstances = types.Select(t => Container.Resolve(t)).Cast<IPaintOnScreen>().ToList(); */ // TODO: Add catch if resolve fails


            /** Working example below
             * 
            var typeNames = PluginLoader.PluginDomainManager.GetAllTypeNamesWhichImplementInterface(typeof(IPaintOnScreen));
            var o = PluginLoader.PluginDomainManager.Instantiate(typeNames[0]);
            var instance = o as IPaintOnScreen;

            var painterInstances = new[] { instance }; 


            if (painterInstances != null)
            {
                foreach (var p in painterInstances)
                {
                    p.StartInvalidateOnTimer += AddClassToInvalidateList;
                    p.StopInvalidateOnTimer += RemoveClassFromInvalidateList;
                    p.Invalidate += Invalidate;
                }
            }
            //m_Painters = painterInstances;

            m_Painters.AddRange(painterInstances);*/
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
