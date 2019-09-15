using Robot.Abstractions;
using Robot.Recordings;
using RobotEditor.PropertyUtils;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace RobotEditor.Inspector
{
    public abstract class CommandProperties : BaseProperties
    {
        [Browsable(false)]
        public virtual Command Command { get; set; }

        [Browsable(false)]
        public override string Title { get { return "Command Properties"; } }

        protected PropertyDescriptorCollection Properties;
        private ICommandFactory CommandFactory;
        public CommandProperties(ICommandFactory CommandFactory) : base(null)
        {
            this.CommandFactory = CommandFactory;

            Properties = TypeDescriptor.GetProperties(this);
        }

        protected const int NumOfCategories = 1;
        protected const int CommandPropertiesCategoryPosition = 1;

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("Move")]
        [DisplayName("Command Type")]
        [Description("Something")]
        [TypeConverter(typeof(CommandNameStringConverter))]
        public string CommandType
        {
            get { return Command.Name; }
            set
            {
                var newCommand = CommandFactory.Create(value, Command);
                BaseHierarchyManager.GetRecordingFromCommand(Command).ReplaceCommand(Command, newCommand);
                Command = newCommand;
            }
        }

        // helper stuff

        protected void AddProperty(DynamicTypeDescriptor dt, string name)
        {
            dt.AddProperty(Properties.Find(name, false));
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

        protected static PropertyInfo GetPropertyInfo(LambdaExpression lambda)
        {
            return (PropertyInfo)GetMemberInfo(lambda);
        }

        protected static MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            return ((MemberExpression)lambda.Body).Member;
        }

        protected static dynamic DynamicCast(Command command)
        {
            return (dynamic)command;
        }
    }
}
