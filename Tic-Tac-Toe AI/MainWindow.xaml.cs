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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tic_Tac_Toe_AI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool inputNeuronsValid => int.TryParse(inputCountTextBox.Text, out int inputCount);
        private bool outputNeuronsValid => int.TryParse(outputCountTextBox.Text, out int outputCount);
        private bool hiddenLayersValid => hiddenLayersListBox.Items.Count > 0;

        public bool CanGenerateNetwork => hiddenLayersValid && inputNeuronsValid && outputNeuronsValid;

        public bool CanAddNewHidden => int.TryParse(newHiddenLayerNeuronCountTextBox.Text, out int hiddenCount);

        public string GenerateNetworkButtonTooltip => string.Format("Input layer: {1}{0}Output layer: {2}{0}Hidden layers: {3}",
            Environment.NewLine, (inputNeuronsValid) ? "Success" : "Fail", (outputNeuronsValid) ? "Success" : "Fail", (hiddenLayersValid) ? "Success" : "Fail");
            
        public MainWindow()
        {
            //vojta kouri pero
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void inputCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("CanGenerateNetwork"));
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("GenerateNetworkButtonTooltip"));
        }

        private void outputCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("CanGenerateNetwork"));
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("GenerateNetworkButtonTooltip"));
        }

        private void newHiddenLayerNeuronCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("CanAddNewHidden"));
        }

        private void newHiddenLayerAddButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("CanGenerateNetwork"));
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("GenerateNetworkButtonTooltip"));
            if (int.TryParse(newHiddenLayerNeuronCountTextBox.Text, out int count))
            {
                hiddenLayersListBox.Items.Add(new HiddenListItem(count));
            }
        }

        private void hiddenNeuronUpButton_Click(object sender, RoutedEventArgs e)
        {
            int index = hiddenLayersListBox.SelectedIndex;
            if (index > 0)
            {
                object temp = hiddenLayersListBox.Items[index];
                hiddenLayersListBox.Items[index] = hiddenLayersListBox.Items[index - 1];
                hiddenLayersListBox.Items[index - 1] = temp;
                hiddenLayersListBox.SelectedIndex = index - 1;
            }
        }

        private void hiddenNeuronRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("CanGenerateNetwork"));
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("GenerateNetworkButtonTooltip"));
            int index = hiddenLayersListBox.SelectedIndex;
            if (index > -1)
            {
                hiddenLayersListBox.Items.RemoveAt(index);
                if (index == hiddenLayersListBox.Items.Count) hiddenLayersListBox.SelectedIndex = index - 1;
                else hiddenLayersListBox.SelectedIndex = index;
            }
        }

        private void hiddenNeuronDownButton_Click(object sender, RoutedEventArgs e)
        {
            int index = hiddenLayersListBox.SelectedIndex;
            if (index > -1 && (hiddenLayersListBox.Items.Count - 1) > index)
            {
                object temp = hiddenLayersListBox.Items[index];
                hiddenLayersListBox.Items[index] = hiddenLayersListBox.Items[index + 1];
                hiddenLayersListBox.Items[index + 1] = temp;
                hiddenLayersListBox.SelectedIndex = index + 1;
            }
        }
    }

    public struct HiddenListItem
    {
        public int Count { get; set; }
        public HiddenListItem(int count)
        {
            Count = count;
        }

        public override string ToString()
        {
            return string.Format("{0} neurons", Count);
        }
    }
}
