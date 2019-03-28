using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNamespace
{
    // This script purpose is for tests only. code inside the main method references a class in a precompilied user
    // dll which is copied into a project by test. Look for "PluginTests"
    public class SomeClassUsingUserPlugin
    {
        public static int Method()
        {
            return TestClassLibrary.Class.Method();
        }
    }
}
