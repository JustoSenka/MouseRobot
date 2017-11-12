using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime.Commands;
using RobotRuntime.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotEditor.Scripts
{
    public class CommandMoveProperties : BaseProperties
    {
        [NonSerialized]
        private CommandMove m_Command;

        [Browsable(false)]
        public override string Title { get { return "Command Properties"; } }

        public CommandMoveProperties(CommandMove settings)
        {
            this.m_Command = settings;
        }

        private const int NumOfCategories = 1;
        private const int CommandPropertiesCategoryPosition = 1;

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("X")]
        public int X
        {
            get { return m_Command.X; }
            set { m_Command.X = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Y")]
        public int Y
        {
            get { return m_Command.Y; }
            set { m_Command.Y = value; }
        }
    }
}
