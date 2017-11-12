using Robot;
using Robot.Scripts;
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
        [Browsable(false)]
        public Command Command { get; private set; }

        private readonly PropertyDescriptorCollection m_Properties;

        [Browsable(false)]
        public override string Title { get { return "Command Properties"; } }

        public CommandProperties(T command)
        {
            m_Properties = TypeDescriptor.GetProperties(this);
            Command = command;
        }

        public override void HideProperties(DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();
            AddProperty(dt, "CommandType");

            if (Command is CommandDown || Command is CommandRelease || Command is CommandPress)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
                AddProperty(dt, "DontMove");
            }
            else if (Command is CommandMove)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
            }
            else if (Command is CommandMoveOnImage)
            {
                AddProperty(dt, "Asset");
                AddProperty(dt, "Smooth");
                AddProperty(dt, "Timeout");
            }
            else if (Command is CommandSleep)
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
            get { return Command.CommandType; }
            set
            {
                var newCommand = CommandFactory.Create(value, Command);
                ScriptManager.Instance.GetScriptFromCommand(Command).ReplaceCommand(Command, newCommand);
                Command = newCommand;
            }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("X")]
        public int X
        {
            get { return DynamicCast(Command).X; }
            set { DynamicCast(Command).X = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Y")]
        public int Y
        {
            get { return DynamicCast(Command).Y; }
            set { DynamicCast(Command).Y = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Dont Move")]
        public bool DontMove
        {
            get { return DynamicCast(Command).DontMove; }
            set { DynamicCast(Command).DontMove = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Time")]
        public int Time
        {
            get { return DynamicCast(Command).Time; }
            set { DynamicCast(Command).Time = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Timeout")]
        public int Timeout
        {
            get { return DynamicCast(Command).Timeout; }
            set { DynamicCast(Command).Timeout = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Smooth")]
        public bool Smooth
        {
            get { return DynamicCast(Command).Smooth; }
            set { DynamicCast(Command).Smooth = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Referenced Asset")]
        public AssetPointer Asset
        {
            get { return DynamicCast(Command).Asset; }
            set { DynamicCast(Command).Asset = value; }
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
