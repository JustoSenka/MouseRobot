using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public interface ICommand
    {
        string Text { set; get; }
        CommandCode Code { set; get; }
        int[] Args { set; get; }

        void Run();
        void ClearMethod();
    }
}
