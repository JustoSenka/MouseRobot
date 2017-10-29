using System.IO;
using System.Linq;
using System.Windows.Forms;
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
            dockPanel.SaveAsXml(GetLayoutPath());
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
                if (persistString.Equals(typeof(TreeViewWindow).ToString()))
                    return Windows[0];
                if (persistString.Equals(typeof(CommandManagerWindow).ToString()))
                    return Windows[1];
                if (persistString.Equals(typeof(ScreenPreviewWindow).ToString()))
                    return Windows[2];
                if (persistString.Equals(typeof(AssetsWindow).ToString()))
                    return Windows[3];

                return null;
            });
        }

        private static bool SavedCopyExists()
        {
            return File.Exists(GetLayoutPath());
        }

        private static string GetLayoutPath()
        {
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), k_LayoutName);
        }
    }
}
