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
using System.Runtime.CompilerServices;

namespace MouseRobotUI
{
    public partial class RecordingForm : Form
    {
        Lazy<IMouseRobot> lazyMR = DependencyInjector.GetLazyMouseRobot();
        
        bool runRecording;
        int x;
        int y;

        bool scriptIsRunning;

        int oldX;
        int oldY;

        MouseButtons oldMB = MouseButtons.None;

        public RecordingForm()
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

                    // Left mouse button down
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

                    // Left mouse button up
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

        private void stopButton_Click(object sender, EventArgs e)
        {
            scriptIsRunning = false;
            lazyMR.Value.StopScript();
        }

        private int TryReadRepeatTimes()
        {
            int repeatTimes;
            try
            {
                repeatTimes = Int32.Parse(countTextBox.Text);
            }
            catch (FormatException fe)
            {
                Console.WriteLine("\"" + countTextBox.Text + "\" is not a valid number.");
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

        private void loadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
            openDialog.Title = "Select a script file to load.";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                lazyMR.Value.Open(openDialog.FileName);
            } 
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Mouse Robot File (*.mrb)|*.mrb";
            saveDialog.Title = "Select a script file to load.";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                lazyMR.Value.Save(saveDialog.FileName);
            } 
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            runRecording = false;
            int repeatTimes = TryReadRepeatTimes();
            TryStartScript(repeatTimes);
        }

        private void emptyButton_Click(object sender, EventArgs e)
        {
            scriptIsRunning = false;
            lazyMR.Value.EmptyScript();
        }
    }

    public static class ExtensionMethods
    {
        public static bool IsNear(this int t, int o)
        {
            return o + 8 > t && o - 8 < t;
        }
    }
}
