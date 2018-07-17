using RobotRuntime.Abstractions;
using System.Diagnostics;

namespace RobotRuntime
{
    public static class LoggerExtension
    {
        /// <summary>
        /// Asserts if expression is true and prints error to log. Returns true if error was printed so caller could return
        /// </summary>
        public static bool AssertIf(this ILogger Logger, bool expression, string errorMsg = "", string description = "")
        {
            if (expression)
            {
                if (errorMsg == "")
                {
                    var CallerType = new StackFrame(2).GetMethod().DeclaringType;
                    errorMsg = "Assert thrown from type: " + CallerType.FullName;
                }
                Logger.Logi(LogType.Error, errorMsg, description);
            }

            return expression;
        }
    }
}
