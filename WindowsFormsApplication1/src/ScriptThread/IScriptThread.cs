using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public interface IScriptThread
    {
        void Start(Script script, int repeatTimes);
    }
}
