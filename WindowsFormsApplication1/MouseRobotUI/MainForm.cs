using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseRobot;

namespace MouseRobotUI
{
    public partial class MainForm : Form
    {
        IMouseRobot mr = new MouseRobotImpl();
        bool keyDown = false;
        decimal timeDown = 0;

        public MainForm()
        {
            
            InitializeComponent();
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyDown)
            {
                return;
            }
            keyDown = true;
            timeDown = (decimal)DateTime.Now.Ticks / (decimal) TimeSpan.TicksPerMillisecond;

            int x = WinAPI.GetCursorPosition().X, y = WinAPI.GetCursorPosition().Y;

            switch (e.KeyData.ToString().ToUpper())
            {
                case "S":
                    Console.WriteLine("Press: (" + x + " ," + y + ")");
                    break;
                case "D":
                    Console.WriteLine("Press: (" + x + " ," + y + ") and sleep");
                    break;
                case "F":
                    Console.WriteLine("Sleep for...");
                    break;
                case "G":
                    Console.WriteLine("Default sleep for " + textBox2.Text);
                    break;
                case "H":
                    Console.WriteLine("Down on: (" + x + " ," + y + ")");
                    break;
                case "J":
                    Console.WriteLine("Move to: (" + x + " ," + y + ")");
                    break;
                case "K":
                    Console.WriteLine("Release");
                    break;
                case "R":
                    Console.WriteLine("Run Script");
                    break;
                case "Q":
                    Console.WriteLine("Empty script");
                    break;
            }
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {
            keyDown = false;

            int x = WinAPI.GetCursorPosition().X, y = WinAPI.GetCursorPosition().Y;
            timeDown = (decimal)DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond - timeDown;

            switch (e.KeyData.ToString().ToUpper())
            {
                case "S":
                    mr.AddCommandPress(x, y);
                    break;
                case "D":
                    mr.AddCommandPress(x, y);
                    mr.AddCommandSleep((int)timeDown);
                    Console.WriteLine("Sleep for..." + timeDown);
                    break;
                case "F":
                    mr.AddCommandSleep((int)timeDown);
                    Console.WriteLine("Sleep for..." + timeDown);
                    break;
                case "G":
                    int sleepTime;
                    try
                    {
                        sleepTime = Int32.Parse(textBox2.Text);
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("\"" + textBox2.Text + "\" is not a valid number.");
                        Console.WriteLine(fe.ToString());
                        sleepTime = 1000;
                    }
                    mr.AddCommandSleep(sleepTime);
                    break;
                case "H":
                    mr.AddCommandDown(x, y);
                    break;
                case "J":
                    mr.AddCommandMove(x, y);
                    break;
                case "K":
                    mr.AddCommandRelease();
                    break;
                case "R":

                    int repeatTimes = TryReadRepeatTimes();
                    TryStartScript(repeatTimes);

                    break;
                case "O":

                    break;
                case "P":

                    break;
                case "Q":
                    mr.EmptyScript();
                    break;
            }
        }

        private int TryReadRepeatTimes()
        {
            int repeatTimes;
            try
            {
                repeatTimes = Int32.Parse(textBox1.Text);
            }
            catch (FormatException fe)
            {
                Console.WriteLine("\"" + textBox1.Text + "\" is not a valid number.");
                repeatTimes = 1;
            }
            return repeatTimes;
        }

        private void TryStartScript(int repeatTimes)
        {
            try
            {
                mr.StartScript(repeatTimes);
            }
            catch (EmptyScriptException ese)
            {
                Console.WriteLine(ese.Message);
            }
        }
    }
}
