using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog(List<int> defaultIntervals, int defaultRound)
        {
            InitializeComponent();

            pomodoroBox.Text = defaultIntervals[(int)PomodoroStatus.Pomodoro].ToString();
            shortBreakBox.Text = defaultIntervals[(int)PomodoroStatus.ShortBreak].ToString();
            longBreakBox.Text = defaultIntervals[(int)PomodoroStatus.LongBreak].ToString();
            roundBox.Text = defaultRound.ToString();
        }

        public List<int> Intervals
        {
            get { return new List<int> { int.Parse(pomodoroBox.Text), int.Parse(shortBreakBox.Text), int.Parse(longBreakBox.Text) }; }
        }

        public int Round
        {
            get { return int.Parse(roundBox.Text); }
        }

        private void NumericBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
