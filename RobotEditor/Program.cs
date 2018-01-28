using Robot;
using RobotEditor.Abstractions;
using System;
using Unity;
using System.Windows.Forms;

namespace RobotEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RobotRuntime.Unity.Init();

            var mainForm = RobotRuntime.Unity.Container.Resolve<IMainForm>();

            Application.Run(mainForm as Form);
        }
    }
}
