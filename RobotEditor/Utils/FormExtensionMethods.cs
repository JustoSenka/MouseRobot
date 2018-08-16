using RobotRuntime;
using System;
using System.Windows.Forms;

namespace RobotEditor
{
    public static class FormExtensionMethods
    {
        public static void BeginInvokeIfCreated<T>(this T control, MethodInvoker MethodInvoker) where T : Control
        {
            if (!control.IsCreatedAndFuctional())
                return;

            try
            {
                control.BeginInvoke(MethodInvoker);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Exception in BeginInvoke for control: " + e.Message);
            }
        }

        public static bool IsCreatedAndFuctional<T>(this T control) where T : Control
        {
            return control.Created && !control.IsDisposed && !control.Disposing;
        }
    }
}
