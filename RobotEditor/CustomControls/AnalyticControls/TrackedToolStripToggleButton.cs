using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Drawing;

namespace RobotEditor.CustomControls
{
    public class TrackedToolStripToggleButton : ToolStripToggleButton
    {
        public TrackedToolStripToggleButton() : base() { }
        public TrackedToolStripToggleButton(string text) : base(text) { }
        public TrackedToolStripToggleButton(Image image) : base(image) { }
        public TrackedToolStripToggleButton(string text, Image image) : base(text, image) { }
        public TrackedToolStripToggleButton(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
        public TrackedToolStripToggleButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }

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
                catch (Exception ex) { Logger.Log(LogType.Error, "Could not get parent form name from ToolStripToggleButton: " + ex.Message); }

                Analytics.PushEvent(category, AnalyticsEvent.A_MenuItemClick, Text);
            }

            base.OnClick(e);
        }
    }
}
