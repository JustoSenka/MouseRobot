using Robot.Abstractions;
using RobotEditor.Utils;
using RobotRuntime;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace RobotEditor.Inspector
{
    public class UnknownCommandProperties : CommandProperties
    {
        public override string HelpTextTitle => "Serialized Text";
        public override string HelpTextContent => SerializedText;

        public UnknownCommandProperties(ICommandFactory CommandFactory) : base(CommandFactory)
        {
            Properties = TypeDescriptor.GetProperties(this);
        }

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();
            AddProperty(dt, "HiddenCommandType");
            AddProperty(dt, "SerializedText");

            var p = Properties.Find("SerializedText", false);
            var a = p.Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
            a?.SetFieldIfExist("description", SerializedText);
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Command Type")]
        public string HiddenCommandType
        {
            get { return Command.Name; }
        }


        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("")]
        [DisplayName("Serialized Text")]
        [Description("")]
        [Editor(typeof(MultiLineTextEditor), typeof(UITypeEditor))]
        public string SerializedText
        {
            get { return DynamicCast(Command).SerializedText; }
        }
    }

    public class MultiLineTextEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _editorService;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            _editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            TextBox textEditorBox = new TextBox();
            textEditorBox.Multiline = true;
            textEditorBox.ScrollBars = ScrollBars.Vertical;
            textEditorBox.Width = 250;
            textEditorBox.Height = 150;
            textEditorBox.BorderStyle = BorderStyle.None;
            textEditorBox.AcceptsReturn = true;
            textEditorBox.Text = value as string;

            _editorService.DropDownControl(textEditorBox);

            return textEditorBox.Text;
        }
    }
}
