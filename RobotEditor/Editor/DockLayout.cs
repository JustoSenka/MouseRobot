using RobotEditor.Windows;
using System;
using System.IO;
using System.Linq;
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
                RestoreDefault();
        }

        public static void RestoreDefault()
        {
            // TODO: Pre-Saved asset or just create by hand windows?
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
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + k_LayoutName;
        }
    }
}
