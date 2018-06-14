using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

        public NeuralNetwork OtherPlayer = null;

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
            NetworkListItem item = new NetworkListItem(newNetworkNameTextBox.Text, int.Parse(inputCountTextBox.Text), int.Parse(outputCountTextBox.Text), ActivationFunctionType.Sigmoid, hidden);
            networksDataGrid.Items.Add(item);
        }

        private void showNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            NeuralNetwork nn = (NeuralNetwork)((Button)sender).Tag;
            NNView view = new NNView(nn);
            view.Owner = this;
            view.Show();
        }

        private void copyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkListItem item = (NetworkListItem)((Button)sender).Tag;
            Clipboard.SetText(item.ToString());
        }

        private async void saveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            button.Content = "Working...";
            NetworkListItem item = (NetworkListItem)button.Tag;
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = item.Name;
            dialog.DefaultExt = ".nni";
            dialog.Filter = "Neural Network Items|*.nni|Neural Networks|*.nn";
            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                FileStream fs = (FileStream)dialog.OpenFile();
                string json;
                if (dialog.FilterIndex == 1)
                {
                    json = await Task.Run(() => JsonConvert.SerializeObject(item, Formatting.Indented));
                }
                else if (dialog.FilterIndex == 2)
                {
                    json = await Task.Run(() => JsonConvert.SerializeObject(item.Network, Formatting.Indented));
                }
                else
                {
                    return;
                }
                byte[] jsonB = new UTF8Encoding(true).GetBytes(json);
                fs.Write(jsonB, 0, jsonB.Length);
                fs.Close();
            }
            button.Content = "File";
            button.IsEnabled = true;
        }

        private void networksDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (e.Key == Key.Delete)
            {
                if (grid.SelectedIndex > 0) grid.Items.RemoveAt(grid.SelectedIndex);
            }
        }

        private async void importNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Neural Network Items|*.nni|Neural Networks|*.nn|All supported items|*.nni;*.nn";
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                Stream[] fss = dialog.OpenFiles();
                foreach (FileStream fs in fss)
                {
                    string ext = System.IO.Path.GetExtension(fs.Name);
                    if (ext == ".nni")
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            NetworkListItem item = await Task.Run(() => JsonConvert.DeserializeObject<NetworkListItem>(reader.ReadToEnd()));
                            networksDataGrid.Items.Add(item);
                        }
                    }
                    else if (ext == ".nn")
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            NeuralNetwork nn = await Task.Run(() => JsonConvert.DeserializeObject<NeuralNetwork>(reader.ReadToEnd()));
                            NetworkListItem item = new NetworkListItem(fs.Name, nn);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("The file {0} has incorrect file extension and couldn't be opened", fs.Name), "File can't be opened", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            SingleGame game = new SingleGame(Player.Blue);
            game.Owner = this;
            game.Show();
        }

        private void playWithNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            NeuralNetwork nn = (NeuralNetwork)button.Tag;
            SingleGame game = new SingleGame(Player.Blue, nn, OtherPlayer);
            game.Owner = this;
            game.Show();
            OtherPlayer = null;
        }

        private void otherNetworkSelectorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            OtherPlayer = (NeuralNetwork)button.Tag;
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

    public class NetworkListItem : INotifyPropertyChanged
    {
        public string Name
        {
            get => _name;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                _name = value;
            }
        }
        public NeuralNetwork Network { get; set; }
        public DateTime GenerationStarted { get; set; }
        public DateTime GenerationFinished { get; set; }
        public string GenTime
        {
            get => _genTime;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GenTime"));
                _genTime = value;
            }
        }
        public int InputNeuronCount
        {
            get => _inputNeuronCount;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputNeuronCount"));
                _inputNeuronCount = value;
            }
        }
        public int OutputNeuronCount
        {
            get => _outputNeuronCount;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutputNeuronCount"));
                _outputNeuronCount = value;
            }
        }
        public int[] HiddenNeuronCounts
        {
            get => _hiddenNeuronCounts;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HiddenNeuronCounts"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HiddenLayerCount"));
                _hiddenNeuronCounts = value;
            }
        }
        public ActivationFunctionType ActivationFunctionType
        {
            get => _activationFunctionType;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActivationFunctionType"));
                _activationFunctionType = value;
            }
        }
        
        public bool Finished { get; set; }
        [JsonIgnore]
        public int HiddenLayerCount => HiddenNeuronCounts.Length;
        [JsonIgnore]
        public NetworkListItem ThisItem => this;


        private BackgroundWorker GenerateNetworkWorker;
        private Timer RefreshItemTimer;

        private string _genTime;
        private string _name;
        private int _inputNeuronCount;
        private int _outputNeuronCount;
        private int[] _hiddenNeuronCounts;
        private ActivationFunctionType _activationFunctionType;

        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkListItem(string name, int inputNeuronCount, int outputNeuronCount, ActivationFunctionType activationFunctionType, params int[] hiddenNeuronCounts)
        {
            Name = name;
            InputNeuronCount = inputNeuronCount;
            OutputNeuronCount = outputNeuronCount;
            HiddenNeuronCounts = hiddenNeuronCounts;
            ActivationFunctionType = activationFunctionType;
            GenerationStarted = DateTime.Now;

            RefreshItemTimer = new Timer();
            RefreshItemTimer.AutoReset = true;
            RefreshItemTimer.Interval = 1000;
            RefreshItemTimer.Elapsed += RefreshItemTimer_Elapsed;

            GenerateNetworkWorker = new BackgroundWorker();
            GenerateNetworkWorker.DoWork += GenerateNetworkWorker_DoWork;
            GenerateNetworkWorker.RunWorkerCompleted += GenerateNetworkWorker_RunWorkerCompleted;
            GenerateNetworkWorker.WorkerReportsProgress = false;
            GenerateNetworkWorker.WorkerSupportsCancellation = false;

            Finished = false;
            GenerateNetworkWorker.RunWorkerAsync();
        }

        public NetworkListItem(string name, NeuralNetwork neuralNetwork)
        {
            Name = name;
            InputNeuronCount = neuralNetwork.InputNeuronCount;
            OutputNeuronCount = neuralNetwork.OutputNeuronCount;
            HiddenNeuronCounts = neuralNetwork.HiddenNeuronCounts.Values.ToArray();
            ActivationFunctionType = neuralNetwork.ActivationFunctionType;
            GenerationStarted = DateTime.Now;
            GenerationFinished = DateTime.Now;
            GenTime = (DateTime.Now - GenerationStarted).ToString(@"hh\:mm\:ss");
            Network = neuralNetwork;
        }

        [JsonConstructor]
        public NetworkListItem(string name, int inputNeuronCount, int outputNeuronCount, TimeSpan genTime, NeuralNetwork network, bool finished, DateTime generationStarted, DateTime generationFinished, int[] hiddenNeuronCounts, ActivationFunctionType activationFunctionType)
        {
            Name = name;
            InputNeuronCount = inputNeuronCount;
            OutputNeuronCount = outputNeuronCount;
            GenTime = genTime.ToString();
            Network = network;
            Finished = finished;
            GenerationStarted = generationStarted;
            GenerationFinished = generationFinished;
            HiddenNeuronCounts = hiddenNeuronCounts;
            ActivationFunctionType = activationFunctionType;
        }

        private void RefreshItemTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GenTime = (DateTime.Now - GenerationStarted).ToString(@"hh\:mm\:ss");
            if (Finished) RefreshItemTimer.Stop();
        }

        private void GenerateNetworkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshItemTimer.Start();
            Network = new NeuralNetwork(InputNeuronCount, _outputNeuronCount, ActivationFunctionType, HiddenNeuronCounts);
        }

        private void GenerateNetworkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GenerationFinished = DateTime.Now;
            Finished = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Finished"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Network"));
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
