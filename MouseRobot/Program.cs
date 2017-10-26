using Robot.Utils.Win32;
using System;
using System.Windows.Forms;

namespace RobotUI
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

            Robot.MouseRobot.Instance.ForceInit();
            //Application.Run(new Form1());
            Application.Run(new MainForm());
        }
    }
}
