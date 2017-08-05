using Robot;
using RobotUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class MainForm : Form
    {
        private TreeViewWindow m_TreeViewWindow;

        private ThemeBase m_CurrentTheme;
        private DeserializeDockContent m_DeserializeDockContent;

        public MainForm()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            ShowSplashScreen(2000);

            CreateWindows();
            CreateWindowDeserializer();

            visualStudioToolStripExtender.DefaultRenderer = new ToolStripProfessionalRenderer();
            SetWindowTheme(this.vS2015DarkTheme1);
        }

        private void CreateWindowDeserializer()
        {
            m_DeserializeDockContent = new DeserializeDockContent((string persistString) =>
            {
                if (persistString.Equals(typeof(TreeViewWindow).ToString()))
                    return m_TreeViewWindow;

                return null;
            });
        }

        private void CreateWindows()
        {
            m_TreeViewWindow = new TreeViewWindow();
        }

        private void SetWindowTheme(ThemeBase theme)
        {

            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.temp.config");
            m_DockPanel.SaveAsXml(configFile);

            CloseAllContents();

            m_CurrentTheme = theme;
            m_DockPanel.Theme = theme;
            
            visualStudioToolStripExtender.SetStyle(menuStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, theme);
            visualStudioToolStripExtender.SetStyle(toolStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, theme);
            visualStudioToolStripExtender.SetStyle(statusStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, theme);

            if (m_DockPanel.Theme.ColorPalette != null)
                statusStrip.BackColor = m_DockPanel.Theme.ColorPalette.MainWindowStatusBarDefault.Background;

            if (File.Exists(configFile))
                m_DockPanel.LoadFromXml(configFile, m_DeserializeDockContent);
        }

        private void EnableVSRenderer(VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            visualStudioToolStripExtender.SetStyle(menuStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(toolStrip, version, theme);
            visualStudioToolStripExtender.SetStyle(statusStrip, version, theme);
        }


        private void CloseAllDocuments()
        {
            foreach (IDockContent document in m_DockPanel.DocumentsToArray())
            {
                document.DockHandler.DockPanel = null;
                document.DockHandler.Close();
            }
        }

        private void CloseAllContents()
        {
            m_TreeViewWindow.DockPanel = null;

            CloseAllDocuments();

            foreach (var window in m_DockPanel.FloatWindows.ToList())
                window.Dispose();

            System.Diagnostics.Debug.Assert(m_DockPanel.Panes.Count == 0);
            System.Diagnostics.Debug.Assert(m_DockPanel.Contents.Count == 0);
            System.Diagnostics.Debug.Assert(m_DockPanel.FloatWindows.Count == 0);
        }

        private void ShowSplashScreen(int millis)
        {
            var splash = new SplashScreen();

            splash.Visible = true;
            splash.TopMost = true;

            Timer timer = new Timer();
            timer.Tick += (sender, e) =>
            {
                splash.Visible = false;
                splash.Dispose();
                timer.Enabled = false;
            };
            timer.Interval = millis;
            timer.Enabled = true;
        }

        #region Menu Items (ScriptManager)
        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.OpenScript(m_TreeViewWindow.treeView);
        }

        private void saveAllScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveAllScripts(m_TreeViewWindow.treeView);
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SaveScript(ScriptManager.Instance.ActiveScript, m_TreeViewWindow.treeView, true);
        }

        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SetSelectedScriptActive(m_TreeViewWindow.treeView);
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.NewScript(m_TreeViewWindow.treeView);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(m_TreeViewWindow.treeView);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.ShowSelectedTreeViewItemInExplorer(m_TreeViewWindow.treeView);
        }
        #endregion

        #region Menu Items (General)
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog(this);
        }

        private void hierarchyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_TreeViewWindow.Show(m_DockPanel);
        }

        private void darkThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_CurrentTheme != vS2015DarkTheme1)
                SetWindowTheme(this.vS2015DarkTheme1);
        }

        private void blueThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_CurrentTheme != vS2015BlueTheme1)
                SetWindowTheme(this.vS2015BlueTheme1);
        }

        private void lightThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_CurrentTheme != vS2015LightTheme1)
                SetWindowTheme(this.vS2015LightTheme1);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

    }
}
