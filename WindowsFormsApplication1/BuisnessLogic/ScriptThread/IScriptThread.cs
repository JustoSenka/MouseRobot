using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public interface IScriptThread
    {
        event EventHandler BreakEvent;
        void Start(IEnumerable<ICommand> list, int repeatTimes);
        void OnBreakEvent(object sender, EventArgs e);
    }
}
