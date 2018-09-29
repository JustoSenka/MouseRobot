using RobotRuntime;
using System;
using System.Windows.Forms;

namespace RobotEditor
{
    public static class FormExtensionMethods
    {
        public static IAsyncResult BeginInvokeIfCreated<T>(this T control, MethodInvoker MethodInvoker) where T : Control
        {
            if (!control.IsCreatedAndFuctional())
                return default(IAsyncResult);

            try
            {
                return control.BeginInvoke(MethodInvoker);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Exception in BeginInvoke for control: " + e.Message);
            }
            return default(IAsyncResult);
        }

        public static void InvokeIfCreated<T>(this T control, MethodInvoker MethodInvoker) where T : Control
        {
            if (!control.IsCreatedAndFuctional())
                return;

            try
            {
                control.Invoke(MethodInvoker);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Exception in Invoke for control: " + e.Message);
            }
        }

        public static bool IsCreatedAndFuctional<T>(this T control) where T : Control
        {
            return control.IsHandleCreated && !control.IsDisposed && !control.Disposing;
        }
    }
}
