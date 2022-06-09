using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Threading;

namespace Pomodoro
{
    public enum PomodoroStatus
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
        PomodoroStatus status = PomodoroStatus.Pomodoro;
        int intervalCounter = 0;
        int finishedCounter = 0;
        int pomodorosPerRound = 4;
        List<int> intervals = new List<int> { 25, 5, 30 };
        TimeSpan intervalTime;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // try to load user settings
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                foreach (var value in Enum.GetValues(typeof(PomodoroStatus)))
                    intervals[(int)value] = int.Parse(appSettings[value.ToString()]);
                pomodorosPerRound = int.Parse(appSettings["Round"]);
            }
            catch
            {
            }

            // DispatcherTimer setup
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            // update UI
            UpdateWithStatus();
            UpdateWithFinishedCounter();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // try to save user settings
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                foreach (var value in Enum.GetValues(typeof(PomodoroStatus)))
                    settings[value.ToString()].Value = intervals[(int)value].ToString();
                settings["Round"].Value = pomodorosPerRound.ToString();

                configFile.Save(ConfigurationSaveMode.Full);
            }
            catch
            {
            }
        }

        private void UpdateWithStatus()
        {
			int interval = intervals[(int)status];
            intervalTime = TimeSpan.FromMinutes(interval);
            timeDisplayText.Text = GetDisplayTime(intervalTime);
            statusText.Text = GetStatusText();
            this.Title = statusText.Text;
        }

        private void UpdateWithFinishedCounter()
        {
            finishText.Text = $"Finished: {finishedCounter}";
        }

        private void NextStatus()
        {
            ++intervalCounter;
            int intervalsPerRound = pomodorosPerRound * 2 + 1;
            intervalCounter %= intervalsPerRound;
            if (intervalCounter == intervalsPerRound - 1)
                status = PomodoroStatus.LongBreak;
            else if (intervalCounter % 2 == 1)
                status = PomodoroStatus.ShortBreak;
            else
                status = PomodoroStatus.Pomodoro;
        }

        private string GetStatusText()
        {
            switch (status)
            {
                case PomodoroStatus.Pomodoro:
                    return "Pomodoro";
                case PomodoroStatus.ShortBreak:
                    return "Short Break";
                case PomodoroStatus.LongBreak:
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

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (intervalTime.TotalSeconds > 0)
            {
                intervalTime = intervalTime.Subtract(TimeSpan.FromSeconds(1));
                timeDisplayText.Text = GetDisplayTime(intervalTime);
                this.Title = $"{timeDisplayText.Text} - {statusText.Text}";
            }
            else
            {
                dispatcherTimer.Stop();
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Time's up!", statusText.Text);

                if (status == PomodoroStatus.Pomodoro)
                {
                    ++finishedCounter;
                    UpdateWithFinishedCounter();
                }

                NextStatus();
                UpdateWithStatus();
                UpdateButtonsDisplay();
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (!dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Start();
            }
            else
            {
                dispatcherTimer.Stop();
            }

			UpdateButtonsDisplay();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            UpdateWithStatus();
            UpdateButtonsDisplay();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            NextStatus();
            UpdateWithStatus();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog settings = new SettingsDialog(intervals, pomodorosPerRound);
            if (settings.ShowDialog() == true)
            {
                if (intervals.SequenceEqual(settings.Intervals) == false || pomodorosPerRound != settings.Round)
                {
                    intervals = settings.Intervals;
                    pomodorosPerRound = settings.Round;

                    intervalCounter = 0;
                    finishedCounter = 0;

                    dispatcherTimer.Stop();
                    UpdateWithStatus();
                    UpdateButtonsDisplay();
                }
            }
        }

        private void UpdateButtonsDisplay()
        {
            if (dispatcherTimer.IsEnabled)
            {
                startButton.Content = "Pause";
                nextButton.IsEnabled = false;
            }
            else
            {
                startButton.Content = "Start";
                nextButton.IsEnabled = true;
            }
        }
    }
}
