using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    public class CorruptedConfigsException : Exception
    {
        public CorruptedConfigsException() : base() { }

        public CorruptedConfigsException(string message) : base(message) { }

        public CorruptedConfigsException(string format, params object[] args) : base(string.Format(format, args)) { }

    }
}
