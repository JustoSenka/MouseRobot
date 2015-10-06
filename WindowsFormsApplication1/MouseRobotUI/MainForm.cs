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
        Lazy<IMouseRobot> lazyMR = DependencyInjector.getLazyMouseRobot();

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
                    //mr.AddCommandPress(x, y);
                    lazyMR.Value.AddCommandPress(x, y);
                    break;
                case "D":
                    lazyMR.Value.AddCommandPress(x, y);
                    lazyMR.Value.AddCommandSleep((int)timeDown);
                    Console.WriteLine("Sleep for..." + timeDown);
                    break;
                case "F":
                    lazyMR.Value.AddCommandSleep((int)timeDown);
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
                    lazyMR.Value.AddCommandSleep(sleepTime);
                    break;
                case "H":
                    lazyMR.Value.AddCommandDown(x, y);
                    break;
                case "J":
                    lazyMR.Value.AddCommandMove(x, y);
                    break;
                case "K":
                    lazyMR.Value.AddCommandRelease();
                    break;
                case "R":

                    int repeatTimes = TryReadRepeatTimes();
                    TryStartScript(repeatTimes);

                    break;
                case "O":
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
                    openDialog.Title = "Select a script file to load.";
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        lazyMR.Value.Open(openDialog.FileName);
                    } 
                    break;
                case "P":
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
                    saveDialog.Title = "Select a script file to load.";
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        lazyMR.Value.Save(saveDialog.FileName);
                    } 
                    break;
                case "Q":
                    lazyMR.Value.EmptyScript();
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lazyMR.Value.StopScript();
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
                Console.WriteLine("\"" + textBox1.Text + "\" is not a valid number." + fe.StackTrace);
                repeatTimes = 1;
            }
            return repeatTimes;
        }

        private void TryStartScript(int repeatTimes)
        {
            try
            {
                lazyMR.Value.StartScript(repeatTimes);
            }
            catch (EmptyScriptException ese)
            {
                Console.WriteLine(ese.Message);
            }
        }
    }
}
