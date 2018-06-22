using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tic_Tac_Toe_AI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SingleGameHubWindow : Window
    {
        private bool inputNeuronsValid => int.TryParse(inputCountTextBox.Text, out int inputCount);
        private bool outputNeuronsValid => int.TryParse(outputCountTextBox.Text, out int outputCount);
        private bool hiddenLayersValid => hiddenLayersListBox.Items.Count > 0;

        public bool CanGenerateNetwork => hiddenLayersValid && inputNeuronsValid && outputNeuronsValid;

        public bool CanAddNewHidden => int.TryParse(newHiddenLayerNeuronCountTextBox.Text, out int hiddenCount);

        public string GenerateNetworkButtonTooltip => string.Format("Input layer: {1}{0}Output layer: {2}{0}Hidden layers: {3}",
            Environment.NewLine, (inputNeuronsValid) ? "Success" : "Fail", (outputNeuronsValid) ? "Success" : "Fail", (hiddenLayersValid) ? "Success" : "Fail");

        public string SaveAllNetworksButtonLabel => string.Format("Save all ({0}) networks to a file", networksDataGrid.Items.Count);

        public NeuralNetwork OtherPlayer
        {
            get
            {
                foreach (object item in networksDataGrid.Items)
                {
                    if (item is NetworkListItem)
                    {
                        NetworkListItem listItem = (NetworkListItem)item;
                        if (listItem.IsOtherPlayerRadioSelected) return listItem.Network;
                    }
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    foreach (object item in networksDataGrid.Items)
                    {
                        if (item is NetworkListItem)
                        {
                            NetworkListItem listItem = (NetworkListItem)item;
                            listItem.IsOtherPlayerRadioSelected = false;
                        }
                    }
                }
            }
        }

        public SingleGameHubWindow()
        {
            InitializeComponent();
            DataContext = this;
            hiddenLayersListBox.Items.Add(new HiddenListItem(600));
            hiddenLayersListBox.Items.Add(new HiddenListItem(600));
            hiddenLayersListBox.Items.Add(new HiddenListItem(600));
            CheckCanGenerateNetwork();
            CheckCanAddNewHiddenLayer();

            Name = "SingleGameHubWindow";
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
            e.Handled = true;
            if (e.Key == Key.Enter)
            {
                ((IInvokeProvider)new ButtonAutomationPeer(newHiddenLayerAddButton).GetPattern(PatternInterface.Invoke)).Invoke();
            }
            else
            {
                CheckCanAddNewHiddenLayer();
            }
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
            saveAllNetworksButtonLabel.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        }

        private void showNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            NeuralNetwork nn = (NeuralNetwork)((Button)sender).Tag;
            NNView view = new NNView(nn);
            view.Owner = this;
            view.Closed += (senderC, eC) => Focus();
            view.Show();
        }

        private void copyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkListItem item = (NetworkListItem)((Button)sender).Tag;
            Clipboard.SetText(item.ToString());
        }

        private void saveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            button.Content = "Working...";
            List<NetworkListItem> items = (List<NetworkListItem>)button.Tag;
            SaveListToFile(items);
            button.Content = "File";
            button.IsEnabled = true;
        }

        private void SaveListToFile(List<NetworkListItem> items)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = items[0].Name;
            dialog.DefaultExt = ".nni";
            dialog.Filter = "Neural Network Items|*.nni|Neural Networks|*.nn";
            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                Stream stream = dialog.OpenFile();
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    object graph = null;
                    if (dialog.FilterIndex == 1)
                    {
                        graph = items;
                    }
                    else if (dialog.FilterIndex == 2)
                    {
                        List<NeuralNetwork> list = new List<NeuralNetwork>();
                        foreach (NetworkListItem item in items)
                        {
                            list.Add(item.Network);
                        }
                        graph = list;
                    }
                    formatter.Serialize(stream, graph);
                }
                catch (SerializationException ex)
                {
                    MessageBox.Show(string.Format("Serialization exception: {0}", ex.Message), "Serialization exception occured", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Wrong file type selected", "Null reference exception occured", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                stream.Flush();
                stream.Close();
                stream = null;
            }
        }

        private void networksDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (e.Key == Key.Delete)
            {
                if (grid.SelectedIndex > 0) grid.Items.RemoveAt(grid.SelectedIndex);
            }
        }

        private void importNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Neural Network Items|*.nni|Neural Networks|*.nn|All supported items|*.nni;*.nn";
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                Stream[] fss = dialog.OpenFiles();
                BinaryFormatter formatter = new BinaryFormatter();
                foreach (FileStream fs in fss)
                {
                    string ext = System.IO.Path.GetExtension(fs.Name);
                    if (ext == ".nni")
                    {
                        List<NetworkListItem> items = (List<NetworkListItem>)formatter.Deserialize(fs);
                        foreach (NetworkListItem item in items)
                        {
                            networksDataGrid.Items.Add(item);
                            item.Finished = true;
                            saveAllNetworksButtonLabel.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                        }
                    }
                    else if (ext == ".nn")
                    {
                        List<NeuralNetwork> nns = (List<NeuralNetwork>)formatter.Deserialize(fs);
                        int i = 0;
                        foreach (NeuralNetwork nn in nns)
                        {
                            NetworkListItem item = new NetworkListItem(string.Format("{0}_{1}", fs.Name, i), nn);
                            networksDataGrid.Items.Add(item);
                            item.Finished = true;
                            saveAllNetworksButtonLabel.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("The file {0} has incorrect file extension and couldn't be opened", fs.Name), "File can't be opened", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    fs.Flush();
                    fs.Close();
                }
            }
        }

        private void playWithNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            NeuralNetwork nn = (NeuralNetwork)button.Tag;
            SingleGameWindow game = new SingleGameWindow(Player.Blue, nn, OtherPlayer);
            game.Owner = this;
            game.Show();
            OtherPlayer = null;
        }

        private void openFreeGameButton_Click(object sender, RoutedEventArgs e)
        {
            SingleGameWindow game = new SingleGameWindow(Player.Blue);
            game.Owner = this;
            game.Show();
        }

        private void saveAllNetworksButton_Click(object sender, RoutedEventArgs e)
        {
            List<NetworkListItem> items = new List<NetworkListItem>();
            foreach (object item in networksDataGrid.Items)
            {
                if (item is NetworkListItem)
                {
                    items.Add((NetworkListItem)item);
                }
            }
            SaveListToFile(items);
        }

        private void generateMultipleNetworksButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(batchGenerationCountTextBox.Text, out int count))
            {
                string oldText = generateMultipleNetworksButtonLabel.Text;
                generateMultipleNetworksButtonLabel.Text = "Working...";
                generateMultipleNetworksButtonLabel.IsEnabled = false;
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = false;
                worker.WorkerSupportsCancellation = false;
                worker.DoWork += (senderW, eW) =>
                {
                    Tuple<int, string, int, int, ActivationFunctionType, int[]> data = (Tuple<int, string, int, int, ActivationFunctionType, int[]>)eW.Argument;
                    List<NetworkListItem> networks = new List<NetworkListItem>();
                    Parallel.For(0, data.Item1, (i) => networks.Add(new NetworkListItem(string.Format("{0}-{1}", data.Item2, i), data.Item3, data.Item4, data.Item5, data.Item6)));
                    eW.Result = networks;

                };
                worker.RunWorkerCompleted += (senderW, eW) =>
                {
                    List<NetworkListItem> networks = (List<NetworkListItem>)eW.Result;
                    SaveListToFile(networks);
                    generateMultipleNetworksButtonLabel.Text = oldText;
                    generateMultipleNetworksButtonLabel.IsEnabled = true;
                };
                int[] hidden = new int[hiddenLayersListBox.Items.Count];
                for (int i = 0; i < hiddenLayersListBox.Items.Count; i++) hidden[i] = ((HiddenListItem)hiddenLayersListBox.Items[i]).Count;
                Tuple<int, string, int, int, ActivationFunctionType, int[]> argument = new Tuple<int, string, int, int, ActivationFunctionType, int[]>(
                    count,
                    newNetworkNameTextBox.Text,
                    int.Parse(inputCountTextBox.Text),
                    int.Parse(outputCountTextBox.Text),
                    ActivationFunctionType.Sigmoid,
                    hidden);
                worker.RunWorkerAsync(argument);
            }
        }

        private void hiddenLayersListBox_KeyUp(object sender, KeyEventArgs e)
        {
            Button button;
            switch (e.Key)
            {
                case Key.Up:
                    button = hiddenNeuronUpButton;
                    break;
                case Key.Delete:
                    button = hiddenNeuronRemoveButton;
                    break;
                case Key.Down:
                    button = hiddenNeuronDownButton;
                    break;
                default:
                    button = null;
                    break;
            }
            if (button != null)
            {
                e.Handled = true;
                ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
            }
        }

        private void hiddenLayersListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    e.Handled = true;
                    break;
                default:
                    break;
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

    [Serializable]
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
                _genTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GenTime"));
            }
        }
        public int InputNeuronCount
        {
            get => _inputNeuronCount;
            set
            {
                _inputNeuronCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputNeuronCount"));
            }
        }
        public int OutputNeuronCount
        {
            get => _outputNeuronCount;
            set
            {
                _outputNeuronCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutputNeuronCount"));
            }
        }
        public int[] HiddenNeuronCounts
        {
            get => _hiddenNeuronCounts;
            set
            {
                _hiddenNeuronCounts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HiddenNeuronCounts"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HiddenLayerCount"));
            }
        }
        public ActivationFunctionType ActivationFunctionType
        {
            get => _activationFunctionType;
            set
            {
                _activationFunctionType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActivationFunctionType"));
            }
        }
        public bool IsOtherPlayerRadioSelected
        {
            get => _isOtherPlayerRadioSelected;
            set
            {
                _isOtherPlayerRadioSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOtherPlayerRadioSelected"));
            }
        }
        public bool Finished
        {
            get => _finished;
            set
            {
                _finished = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Finished"));
            }
        }

        [JsonIgnore]
        public int HiddenLayerCount => HiddenNeuronCounts.Length;
        [JsonIgnore]
        public NetworkListItem ThisItem => this;
        [JsonIgnore]
        public List<NetworkListItem> ThisAsList
        {
            get
            {
                List<NetworkListItem> list = new List<NetworkListItem>();
                list.Add(this);
                return list;
            }
        }

        [NonSerialized]
        private BackgroundWorker GenerateNetworkWorker;
        [NonSerialized]
        private Timer RefreshItemTimer;

        private string _genTime;
        private string _name;
        private int _inputNeuronCount;
        private int _outputNeuronCount;
        private int[] _hiddenNeuronCounts;
        private ActivationFunctionType _activationFunctionType;
        private bool _isOtherPlayerRadioSelected;
        private bool _finished;

        [field: NonSerialized]
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
    }
}
