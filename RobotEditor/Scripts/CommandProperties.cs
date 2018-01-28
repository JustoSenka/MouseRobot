using Robot;
using Robot.Abstractions;
using Robot.Scripts;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Assets;
using RobotRuntime.Commands;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq.Expressions;
using System.Reflection;

namespace RobotEditor.Scripts
{
    public class CommandProperties<T> : BaseProperties where T : Command
    {
        [Browsable(false)]
        public Command m_Command { get; private set; }

        private readonly PropertyDescriptorCollection m_Properties;

        [Browsable(false)]
        public override string Title { get { return "Command Properties"; } }

        private IAssetManager AssetManager;
        private IScriptManager ScriptManager;
        public CommandProperties(T command, IAssetManager AssetManager, IScriptManager ScriptManager)
        {
            this.ScriptManager = ScriptManager;
            this.AssetManager = AssetManager;

            m_Properties = TypeDescriptor.GetProperties(this);
            m_Command = command;
        }

        public override void HideProperties(DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();
            AddProperty(dt, "CommandType");

            if (m_Command is CommandDown || m_Command is CommandRelease || m_Command is CommandPress)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
                AddProperty(dt, "DontMove");
            }
            else if (m_Command is CommandMove)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
            }
            else if (m_Command is CommandForImage || m_Command is CommandForeachImage)
            {
                AddProperty(dt, "Asset");
                AddProperty(dt, "Timeout");
            }
            else if (m_Command is CommandSleep)
            {
                AddProperty(dt, "Time");
            }
        }

        private const int NumOfCategories = 1;
        private const int CommandPropertiesCategoryPosition = 1;

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Command Type")]
        public CommandType CommandType
        {
            get { return m_Command.CommandType; }
            set
            {
                var newCommand = CommandFactory.Create(value, m_Command);
                ScriptManager.GetScriptFromCommand(m_Command).ReplaceCommand(m_Command, newCommand);
                m_Command = newCommand;
            }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("X")]
        public int X
        {
            get { return DynamicCast(m_Command).X; }
            set { DynamicCast(m_Command).X = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Y")]
        public int Y
        {
            get { return DynamicCast(m_Command).Y; }
            set { DynamicCast(m_Command).Y = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Dont Move")]
        public bool DontMove
        {
            get { return DynamicCast(m_Command).DontMove; }
            set { DynamicCast(m_Command).DontMove = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Time")]
        public int Time
        {
            get { return DynamicCast(m_Command).Time; }
            set { DynamicCast(m_Command).Time = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Timeout")]
        public int Timeout
        {
            get { return DynamicCast(m_Command).Timeout; }
            set { DynamicCast(m_Command).Timeout = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Smooth")]
        public bool Smooth
        {
            get { return DynamicCast(m_Command).Smooth; }
            set { DynamicCast(m_Command).Smooth = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Referenced Asset")]
        [TypeConverter(typeof(AssetGUIDImageStringConverter))]
        [Editor(typeof(AssetGUIDImageUITypeEditor), typeof(UITypeEditor))]
        public string Asset
        {
            get
            {
                var guid = DynamicCast(m_Command).Asset;
                var path = "abvd";// AssetGuidManager.GetPath(guid);
                return (path == null || path == "") ? "..." : Commons.GetName(path);
            }
            set
            {
                Asset asset = AssetManager.GetAsset(AssetManager.ImageFolder, value);
                if (asset != null)
                    DynamicCast(m_Command).Asset = asset.Guid;
            }
        }

        private dynamic DynamicCast(Command command)
        {
            return (dynamic)command;
        }

        private void AddProperty(DynamicTypeDescriptor dt, string name)
        {
            dt.AddProperty(m_Properties.Find(name, false));
        }

        /// <summary>
        /// This string will get property without using magic strings
        /// Not used now, too complex, maybe in future will be used
        /// Property<CommandProperties<Command>, int>((p => p.X)).Name;
        /// </summary>
        public static PropertyInfo Property<TClass, K>(Expression<Func<TClass, K>> m)
        {
            return GetPropertyInfo(m);
        }

        static PropertyInfo GetPropertyInfo(LambdaExpression lambda)
        {
            return (PropertyInfo)GetMemberInfo(lambda);
        }

        static MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            return ((MemberExpression)lambda.Body).Member;
        }
    }
}
