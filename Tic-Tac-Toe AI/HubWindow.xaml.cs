using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Tic_Tac_Toe_AI
{
    /// <summary>
    /// Interaction logic for HubWindow.xaml
    /// </summary>
    public partial class HubWindow : Window, INotifyPropertyChanged
    {
        private SingleGameHubWindow SingleGameHub;
        private AITrainingHubWindow AITrainingHub;

        public event PropertyChangedEventHandler PropertyChanged;

        public string OpenSingleGameHubWindowButton_Content => string.Format("{0} Single Game Hub", SingleGameHub != null ? "Focus" : "Open");
        public string OpenAITrainingHubWindowButton_Content => string.Format("{0} AI Training Hub", AITrainingHub != null ? "Focus" : "Open");

        public HubWindow()
        {
            InitializeComponent();
        }

        private void openSingleGameHubWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (SingleGameHub != null) SingleGameHub.Focus();
            else
            {
                SingleGameHub = new SingleGameHubWindow();
                SingleGameHub.Closed += SingleGameHub_Closed;
                SingleGameHub.Owner = this;
                SingleGameHub.Show();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpenSingleGameHubWindowButton_Content"));
        }

        private void SingleGameHub_Closed(object sender, EventArgs e)
        {
            Focus();
            SingleGameHub = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpenSingleGameHubWindowButton_Content"));
        }

        private void openAITrainingHubWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (AITrainingHub != null) AITrainingHub.Focus();
            else
            {
                AITrainingHub = new AITrainingHubWindow();
                AITrainingHub.Closed += AITrainingHub_Closed;
                AITrainingHub.Owner = this;
                AITrainingHub.Show();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpenAITrainingHubWindowButton_Content"));
        }

        private void AITrainingHub_Closed(object sender, EventArgs e)
        {
            Focus();
            AITrainingHub = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpenAITrainingHubWindowButton_Content"));
        }
    }
}
