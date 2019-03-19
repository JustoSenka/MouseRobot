using RobotRuntime;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor.Editor
{
    static class DockLayout
    {
        private static DockContent[] m_Windows;
        public static DockContent[] Windows
        {
            private get
            {
                return m_Windows;
            }
            set
            {
                m_Windows = value;
                CreateWindowDeserializer();
            }
        }

        private const string k_LayoutName = "DockLayout.config";
        private static DeserializeDockContent m_DeserializeDockContent;

        public static void Save(DockPanel dockPanel)
        {
            var fileInfo = new FileInfo(GetLayoutPath());
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            dockPanel.SaveAsXml(fileInfo.FullName);
        }

        public static void Restore(DockPanel dockPanel)
        {
            if (Windows != null && Windows.Length > 0)
                CloseAllContents(dockPanel);

            if (SavedCopyExists())
                dockPanel.LoadFromXml(GetLayoutPath(), m_DeserializeDockContent);
            else
                RestoreDefault(dockPanel);
        }

        public static void RestoreDefault(DockPanel dockPanel)
        {
            try
            {
                var byteArray = Encoding.Unicode.GetBytes(Properties.Resources.DefaultDockLayout);
                using (var s = new MemoryStream(byteArray))
                    dockPanel.LoadFromXml(s, m_DeserializeDockContent, true);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot deserialize default layout: " + e.Message);
            }
        }

        public static void CloseAllContents(DockPanel dockPanel)
        {
            foreach (var w in Windows)
                w.DockPanel = null;

            foreach (var window in dockPanel.FloatWindows.ToList())
                window.Dispose();

            foreach (IDockContent document in dockPanel.DocumentsToArray())
            {
                document.DockHandler.DockPanel = null;
                document.DockHandler.Close();
            }

            System.Diagnostics.Debug.Assert(dockPanel.Panes.Count == 0);
            System.Diagnostics.Debug.Assert(dockPanel.Contents.Count == 0);
            System.Diagnostics.Debug.Assert(dockPanel.FloatWindows.Count == 0);
        }

        private static void CreateWindowDeserializer()
        {
            m_DeserializeDockContent = new DeserializeDockContent((string persistString) =>
            {
                foreach (var win in Windows)
                {
                    if (persistString.Equals(win.GetType().ToString()))
                        return win;
                }

                return null;
            });
        }

        private static bool SavedCopyExists()
        {
            return File.Exists(GetLayoutPath());
        }

        private static string GetLayoutPath()
        {
            return Path.Combine(Paths.RoamingAppdataPath, k_LayoutName);
        }
    }
}
