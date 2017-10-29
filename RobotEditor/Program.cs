using Robot;
using System;
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

            MouseRobot.Instance.ForceInit();
            //Application.Run(new Form1());
            Application.Run(new MainForm());
        }
    }
}
