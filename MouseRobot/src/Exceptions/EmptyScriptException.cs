using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    public class EmptyScriptException : Exception
    {
        public EmptyScriptException() : base() { }

        public EmptyScriptException(string message) : base(message) { }

        public EmptyScriptException(string format, params object[] args) : base(string.Format(format, args)) { }

    }
}
