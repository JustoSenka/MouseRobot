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
using System.Threading;

namespace MouseRobotUI
{
    public partial class MainForm2 : Form
    {
        Lazy<IMouseRobot> lazyMR = DependencyInjector.GetLazyMouseRobot();

        bool scriptIsRunning;
        volatile bool runRecording;
        bool keyDown = false;
        decimal timeDown = 0;

        volatile int x;
        int oldX;
        volatile int y; 
        int oldY;
        MouseButtons oldMB = MouseButtons.None;

        public MainForm2()
        {
            InitializeComponent();

            StartPassiveMousePositionCheckerThread();
        }

        private void StartPassiveMousePositionCheckerThread()
        {
            new Thread(() =>
            {
                while (true)
                {
                    x = WinAPI.GetCursorPosition().X;
                    y = WinAPI.GetCursorPosition().Y;

                    if (x < 5 && y < 5 && !runRecording)
                    {
                        StartRecordingThread();
                        Invoke(new Action(() => { this.WindowState = FormWindowState.Minimized; }));
                        WinAPI.Show(WinAPI.GetConsole(), WinAPI.SW_HIDE);
                    }

                    if (x > 800 && y < 5)
                    {
                        runRecording = false;
                        Invoke(new Action(() => { this.WindowState = FormWindowState.Normal; }));
                        WinAPI.Show(WinAPI.GetConsole(), WinAPI.SW_SHOW);
                    }

                    // If script break, show windows.
                    if (y < 5 && !runRecording)
                    {
                        Invoke(new Action(() => { this.WindowState = FormWindowState.Normal; }));
                        WinAPI.Show(WinAPI.GetConsole(), WinAPI.SW_SHOW);
                    }

                    Thread.Sleep(30);
                }
            }).Start();
        }

        private void StartRecordingThread()
        {
            runRecording = true;
            Console.WriteLine("Recording started.");
            new Thread(() =>
            {
                while (runRecording)
                {
                    x = WinAPI.GetCursorPosition().X;
                    y = WinAPI.GetCursorPosition().Y;

                    // Left down
                    if (!oldMB.HasFlag(MouseButtons.Left) && MouseButtons.HasFlag(MouseButtons.Left))
                    {
                        if (!x.IsNear(oldX) || !y.IsNear(oldY))
                        {
                            lazyMR.Value.AddCommandMove(x, y);
                        }
                        lazyMR.Value.AddCommandDown(x, y);

                        oldMB = MouseButtons;
                        oldX = x;
                        oldY = y;
                    }

                    // Left up
                    else if (oldMB.HasFlag(MouseButtons.Left) && !MouseButtons.HasFlag(MouseButtons.Left))
                    {
                        if (!x.IsNear(oldX) || !y.IsNear(oldY))
                        {
                            lazyMR.Value.AddCommandMove(x, y);
                        }
                        lazyMR.Value.AddCommandRelease();

                        oldMB = MouseButtons;
                        oldX = x;
                        oldY = y;
                    }

                    // Does nothing
                    else if (x.IsNear(oldX) && y.IsNear(oldY))
                    {
                        lazyMR.Value.AddCommandSleep(30);
                    }

                    Thread.Sleep(30);
                }
                Console.WriteLine("Recording ended.");
            }).Start();
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
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {
            keyDown = false;

            int x = WinAPI.GetCursorPosition().X, y = WinAPI.GetCursorPosition().Y;
            timeDown = (decimal)DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond - timeDown;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            scriptIsRunning = false;
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
                Console.WriteLine("\"" + textBox1.Text + "\" is not a valid number.");
                repeatTimes = 1;
            }
            return repeatTimes;
        }

        private void TryStartScript(int repeatTimes)
        {
            try
            {
                lazyMR.Value.StartScript(repeatTimes);

                Invoke(new Action(() => { this.WindowState = FormWindowState.Minimized; }));
                WinAPI.Show(WinAPI.GetConsole(), WinAPI.SW_HIDE);
            }
            catch (EmptyScriptException ese)
            {
                Console.WriteLine(ese.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
            openDialog.Title = "Select a script file to load.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                lazyMR.Value.Open(openDialog.FileName);
            } 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
            saveDialog.Title = "Select a script file to load.";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                lazyMR.Value.Save(saveDialog.FileName);
            } 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            runRecording = false;
            int repeatTimes = TryReadRepeatTimes();
            TryStartScript(repeatTimes);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            scriptIsRunning = false;
            lazyMR.Value.EmptyScript();
        }
    }

    public static class ExtentionMethods
    {
        public static bool IsNear(this int t, int o)
        {
            return o + 8 > t && o - 8 < t;
        }
    }
}
