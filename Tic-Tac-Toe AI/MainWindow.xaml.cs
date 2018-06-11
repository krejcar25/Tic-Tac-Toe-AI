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
    public partial class MainWindow : Window
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
            InitializeComponent();
            DataContext = this;
            hiddenLayersListBox.Items.Add(new HiddenListItem(3));
            hiddenLayersListBox.Items.Add(new HiddenListItem(4));
            CheckCanGenerateNetwork();
            CheckCanAddNewHiddenLayer();
        }

        private void CheckCanGenerateNetwork()
        {
            generateNetworkButton.IsEnabled = CanGenerateNetwork;
            generateNetworkButton.ToolTip = GenerateNetworkButtonTooltip;
        }

        private void CheckCanAddNewHiddenLayer()
        {
            newHiddenLayerAddButton.IsEnabled = CanAddNewHidden;
        }

        private void inputCountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            CheckCanGenerateNetwork();
        }

        private void outputCountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            CheckCanGenerateNetwork();
        }

        private void newHiddenLayerNeuronCountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            CheckCanAddNewHiddenLayer();
        }

        private void newHiddenLayerAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(newHiddenLayerNeuronCountTextBox.Text, out int count))
            {
                hiddenLayersListBox.Items.Add(new HiddenListItem(count));
            }
            CheckCanGenerateNetwork();
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
            CheckCanGenerateNetwork();
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

        private void generateNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            int[] hidden = new int[hiddenLayersListBox.Items.Count];
            for (int i = 0; i < hiddenLayersListBox.Items.Count; i++) hidden[i] = ((HiddenListItem)hiddenLayersListBox.Items[i]).Count;
            NeuralNetwork nn = new NeuralNetwork(int.Parse(inputCountTextBox.Text), int.Parse(outputCountTextBox.Text), ActivationFunctionType.Sigmoid, hidden);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) Clipboard.SetText(nn.ToString());
            NNView view = new NNView(nn);
            view.Show();
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
