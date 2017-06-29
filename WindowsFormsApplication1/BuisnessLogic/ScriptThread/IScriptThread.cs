using MouseRobotUI.BuisnessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public delegate void MyEventHandler(object sender, CustomEventArgs e);

    public interface IScriptThread
    {
        event MyEventHandler BreakEvent;
        void Start(Script script, int repeatTimes);
        void OnBreakEvent(object sender, CustomEventArgs e);
    }
}
