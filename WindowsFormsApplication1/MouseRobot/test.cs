using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    [Serializable]
    class test
    {
        int a, b, c;
        public test(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override string ToString()
        {
            return a + " " + b + " " + c;
        }
    }
}
