using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pomodoro
{
    public enum Status
    {
        Pomodoro,
        ShortBreak,
        LongBreak
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Status status = Status.Pomodoro;
        int intervalCounter = 0;
        int finishedCounter = 0;
        int pomodorosPerRound = 4;
        List<int> intervals = new List<int> { 25, 5, 30 };
        TimeSpan intervalTime;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // update UI
            UpdateWithStatus();
            UpdateWithFinishedCounter();
        }

        private void UpdateWithStatus()
        {
            int interval = intervals[(int)status];
            intervalTime = TimeSpan.FromMinutes(interval);
            timeDisplayText.Text = GetDisplayTime(intervalTime);
            statusText.Text = GetStatusText();
        }

        private void UpdateWithFinishedCounter()
        {
            finishText.Text = $"Finished: {finishedCounter}";
        }

        private string GetStatusText()
        {
            switch (status)
            {
                case Status.Pomodoro:
                    return "Pomodoro";
                case Status.ShortBreak:
                    return "Short Break";
                case Status.LongBreak:
                    return "Long Break";
                default:
                    break;
            }
            return String.Empty;
        }

        private string GetDisplayTime(TimeSpan time)
        {
            return String.Format($@"{(int)time.TotalMinutes:00}:{time.Seconds:00}");
        }
    }
}
