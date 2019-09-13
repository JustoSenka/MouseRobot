using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.CustomControls
{
    public class TrackedToolStripButton : ToolStripButton
    {
        public TrackedToolStripButton() : base() { }
        public TrackedToolStripButton(string text) : base(text) { }
        public TrackedToolStripButton(Image image) : base(image) { }
        public TrackedToolStripButton(string text, Image image) : base(text, image) { }
        public TrackedToolStripButton(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
        public TrackedToolStripButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }

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

                    category = name.IsEmpty() ? category : name;
                }
                catch (Exception ex) { Logger.Log(LogType.Error, "Could not get parent form name from ToolStripButton: " + ex.Message); }

                Analytics.PushEvent(category, AnalyticsEvent.A_MenuItemClick, Text);
            }

            base.OnClick(e);
        }
    }
}
