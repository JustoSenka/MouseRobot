using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace RobotEditor.CustomControls
{
    public class TrackedToolStripMenuItem : ToolStripMenuItem
    {
        public TrackedToolStripMenuItem() : base() { }
        public TrackedToolStripMenuItem(string text) : base(text) { }
        public TrackedToolStripMenuItem(Image image) : base(image) { }
        public TrackedToolStripMenuItem(string text, Image image) : base(text, image) { }
        public TrackedToolStripMenuItem(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
        public TrackedToolStripMenuItem(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }
        public TrackedToolStripMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys) : base(text, image, onClick, shortcutKeys) { }
        public TrackedToolStripMenuItem(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems) { }

#pragma warning disable 649
        [RequestStaticDependency(typeof(IAnalytics))]
        private static IAnalytics Analytics;
#pragma warning restore 649 

        public bool AnalyticsEnabled { get; set; } = true;

        protected override void OnClick(EventArgs e)
        {
            if (AnalyticsEnabled && Analytics.IsEnabled)
            {
                var category = "UnknownForm";

                try
                {
                    var name = this.Parent?.Parent?.FindForm()?.GetType().Name;
                    name = name.IsEmpty() ? this.Parent?.TopLevelControl?.FindForm()?.GetType().Name : name;
                    name = name.IsEmpty() ? (this.Parent as ContextMenuStrip)?.SourceControl?.FindForm()?.GetType().Name : name;

                    if (this.Parent?.Items.Count > 0)
                        name = name.IsEmpty() ? this.Parent?.Items[0].OwnerItem?.GetCurrentParent()?.FindForm()?.Name : name;

                    category = name.IsEmpty() ? category : name;
                }
                catch (Exception ex) { Logger.Log(LogType.Error, "Could not get parent form name from ToolStripMenuItem: " + ex.Message); }

                Analytics.PushEvent(category, AnalyticsEvent.A_MenuItemClick, Text);
            }

            base.OnClick(e);
        }
    }
}
