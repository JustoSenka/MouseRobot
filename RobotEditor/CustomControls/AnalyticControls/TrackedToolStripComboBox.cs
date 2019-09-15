using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Windows.Forms;

namespace RobotEditor.CustomControls
{
    public class TrackedToolStripComboBox : ToolStripComboBox
    {
        public TrackedToolStripComboBox() : base() { }
        public TrackedToolStripComboBox(Control ctrl) : base(ctrl) { }
        public TrackedToolStripComboBox(string name) : base(name) { }

#pragma warning disable 649
        [RequestStaticDependency(typeof(IAnalytics))]
        private static IAnalytics Analytics;
#pragma warning restore 649 

        public bool AnalyticsEnabled { get; set; } = true;

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            var selected = SelectedItem.ToString();
            if (AnalyticsEnabled && Analytics.IsEnabled)
            {
                var category = "UnknownForm";

                try
                {
                    var name = this.Parent?.Parent?.FindForm()?.GetType().Name;
                    name = name.IsEmpty() ? this.Parent?.TopLevelControl?.FindForm()?.GetType().Name : name;

                    category = name.IsEmpty() ? category : name;
                }
                catch (Exception ex) { Logger.Log(LogType.Error, "Could not get parent form name from ToolStripComboBox: " + ex.Message); }

                Analytics.PushEvent(category, AnalyticsEvent.A_MenuItemClick, selected);
            }

            base.OnSelectedIndexChanged(e);
        }
    }
}
