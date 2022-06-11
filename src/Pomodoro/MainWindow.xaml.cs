using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PomodoroStatus status = PomodoroStatus.Pomodoro;
        private int intervalCounter = 0;
        private int finishedCounter = 0;
        private int pomodorosPerRound = 4;
        private List<int> intervals = new List<int> { 25, 5, 30 };
        private TimeSpan intervalTime;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PomodoroStatus Status
        {
            get { return this.status; }
            set { this.status = value; NotifyPropertyChanged(); }
        }

        public int FinishedCounter
        {
            get { return this.finishedCounter; }
            set { this.finishedCounter = value; NotifyPropertyChanged(); }
        }

        public TimeSpan IntervalTime
        {
            get { return this.intervalTime; }
            set { this.intervalTime = value; NotifyPropertyChanged(); }
        }

        public static string GetStatusText(PomodoroStatus pomodoroStatus)
        {
            switch (pomodoroStatus)
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

        public static string GetDisplayTime(TimeSpan time)
        {
            return String.Format($@"{(int)time.TotalMinutes:00}:{time.Seconds:00}");
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
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

            ResetStatusInterval();
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

        private void ResetStatusInterval()
        {
			int interval = intervals[(int)Status];
            IntervalTime = TimeSpan.FromMinutes(interval);
        }

        private void NextStatus()
        {
            ++intervalCounter;
            int intervalsPerRound = pomodorosPerRound * 2 + 1;
            intervalCounter %= intervalsPerRound;
            if (intervalCounter == intervalsPerRound - 1)
                Status = PomodoroStatus.LongBreak;
            else if (intervalCounter % 2 == 1)
                Status = PomodoroStatus.ShortBreak;
            else
                Status = PomodoroStatus.Pomodoro;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IntervalTime.TotalSeconds > 0)
            {
                IntervalTime = IntervalTime.Subtract(TimeSpan.FromSeconds(1));
            }
            else
            {
                dispatcherTimer.Stop();
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Time's up!", statusText.Text);

                if (Status == PomodoroStatus.Pomodoro)
                {
                    ++FinishedCounter;
                }

                NextStatus();
                ResetStatusInterval();
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
            ResetStatusInterval();
            UpdateButtonsDisplay();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            NextStatus();
            ResetStatusInterval();
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
                    FinishedCounter = 0;

                    dispatcherTimer.Stop();
                    ResetStatusInterval();
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

    public class PomodoroStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return MainWindow.GetStatusText((PomodoroStatus)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class TimerToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan time)
                return MainWindow.GetDisplayTime(time);
            else
                return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
