using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Commands;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace RobotEditor.Scripts
{
    public class CommandProperties<T> : BaseProperties where T : Command
    {
        private Command m_Command;
        private readonly PropertyDescriptorCollection m_Properties;

        [Browsable(false)]
        public override string Title { get { return "Command Properties"; } }

        public CommandProperties(T command)
        {
            m_Properties = TypeDescriptor.GetProperties(this);
            m_Command = command;
        }

        public override void HideProperties(DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();

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
            else if (m_Command is CommandMoveOnImage)
            {
                AddProperty(dt, "Asset");
                AddProperty(dt, "Smooth");
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
        [DefaultValue(0)]
        [DisplayName("Dont Move")]
        public bool DontMove
        {
            get { return DynamicCast(m_Command).DontMove; }
            set { DynamicCast(m_Command).DontMove = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Time")]
        public bool Time
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
        [DefaultValue(0)]
        [DisplayName("Smooth")]
        public bool Smooth
        {
            get { return DynamicCast(m_Command).Smooth; }
            set { DynamicCast(m_Command).Smooth = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Referenced Asset")]
        public AssetPointer Asset
        {
            get { return DynamicCast(m_Command).Asset; }
            set { DynamicCast(m_Command).Asset = value; }
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
