using BrightIdeasSoftware;
using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Abstractions;
using RobotEditor.Windows.Base;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace RobotEditor.Design
{
    [RegisterTypeToContainer(typeof(IFontManager))]
    public class FontManager : IFontManager
    {
        public IList<Form> Forms { get; } = new List<Form>();
        public IList<Control> Controls { get; } = new List<Control>();

        private IList<(Form Form, Type FieldType, Control Value)> m_CollectedControls = new List<(Form, Type, Control)>();

        private const BindingFlags k_BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly ISettingsManager SettingsManager;
        private readonly IProfiler Profiler;
        public FontManager(ISettingsManager SettingsManager, IProfiler Profiler)
        {
            this.SettingsManager = SettingsManager;
            this.Profiler = Profiler;

            SettingsManager.SettingsRestored += UpdateFontsInternal;
            SettingsManager.SettingsModified += OnSettingsModified;
        }

        public void ForceUpdateFonts()
        {
            Profiler.Begin("FontManager_ForceUpdateFonts", () =>
            {
                // Collect all controls of Form and project into map of Form - FieldType - FieldValue
                m_CollectedControls = Forms.SelectMany(f => f.GetType().GetFields(k_BindingFlags)
                    .Where(field => field.FieldType == typeof(PropertyGrid) || field.FieldType == typeof(TreeListView))
                    .Select(field => (Form: f, Field: field)))
                    .Select(tuple => (tuple.Form, tuple.Field.FieldType, Value: tuple.Field.GetValue(tuple.Form) as Control)).ToList();

                UpdateFontsInternal();
            });
        }

        private void OnSettingsModified(BaseSettings settings)
        {
            if (settings.GetType() == typeof(DesignSettings))
                UpdateFontsInternal();
        }

        private void UpdateFontsInternal()
        {
            var settings = SettingsManager.GetSettings<DesignSettings>();
            if (settings == null)
                return;

            var fontToUse = settings.DefaultWindowFont;
            foreach (var (Form, FieldType, Control) in m_CollectedControls)
            {
                if (Control == null)
                {
                    Logger.Log(LogType.Error, $"Form: {Form}, FieldType: {FieldType}, actual control value was null.");
                    continue;
                }

                if (Form is BaseHierarchyWindow && Control is TreeListView)
                    fontToUse = settings.HierarchyWindowsFont;

                else if (Form is IAssetsWindow && Control is TreeListView)
                    fontToUse = settings.AssetsWindowFont;

                else if (Form is ITestRunnerWindow && Control is TreeListView)
                    fontToUse = settings.TestRunnerWindowFont;

                else
                    fontToUse = settings.DefaultWindowFont;

                Control.Font = fontToUse;
            }
        }
    }
}
