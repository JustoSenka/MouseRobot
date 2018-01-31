using RobotRuntime.Abstractions;

namespace RobotRuntime
{
    public partial class Logger
    {
        public static ILogger Instance;

        public static void Log(LogType logType, string str)
        {
            Instance.Logi(logType, str);
        }

        public static void Log(LogType logType, string obj, string description)
        {
            Instance.Logi(logType, obj, description);
        }
    }
}
