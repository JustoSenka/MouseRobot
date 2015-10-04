using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MouseRobot
{
    public class ScriptThread
    {
        public static void Start(Action method)
        {
            new Thread(delegate()
            {
                method.Invoke();
            }).Start();
        }
    }
}
