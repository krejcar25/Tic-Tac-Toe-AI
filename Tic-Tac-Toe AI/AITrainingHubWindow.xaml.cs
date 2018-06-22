using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
    /// Interaction logic for AITrainingHubWindow.xaml
    /// </summary>
    public partial class AITrainingHubWindow : Window
    {
        AITrainingHubViewModel ViewModel => (AITrainingHubViewModel)DataContext;

        private readonly BackgroundWorker evolutionWorker = new BackgroundWorker();

        FileStream networkFile;

        List<List<NeuralNetwork>> evolution = new List<List<NeuralNetwork>>();

        public AITrainingHubWindow()
        {
            InitializeComponent();
            evolutionWorker.DoWork += EvolutionWorker_DoWork;
            evolutionWorker.RunWorkerCompleted += EvolutionWorker_RunWorkerCompleted;
            evolutionWorker.ProgressChanged += EvolutionWorker_ProgressChanged;
            evolutionWorker.WorkerReportsProgress = true;
            evolutionWorker.WorkerSupportsCancellation = true;
        }

        private void EvolutionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ViewModel.CurrentProgressGeneration += 1;
            ViewModel.CurrentProgressOverall += 1;
        }

        private void EvolutionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void EvolutionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            BinaryFormatter formatter = new BinaryFormatter();
            string ext = System.IO.Path.GetExtension(networkFile.Name);
            List<NeuralNetwork> networks;
            if (ext == ".nni")
            {
                List<NetworkListItem> items = (List<NetworkListItem>)formatter.Deserialize(networkFile);
                networks = (List<NeuralNetwork>)items.Select(item => item.Network);
            }
            else if (ext == ".nn")
            {
                networks = (List<NeuralNetwork>)formatter.Deserialize(networkFile);
            }
            else
            {
                MessageBox.Show(string.Format("The file {0} has incorrect file extension and couldn't be opened", networkFile.Name), "File can't be opened", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            networkFile.Flush();
            networkFile.Close();
            networkFile = null;

            ViewModel.GenerationProgressBarMaximum = networks.Count * (networks.Count - 1);
            ViewModel.OverallProgressBarMaximum = networks.Count * (networks.Count - 1) * ViewModel.DesiredGenerationCount;

            GenerationScore scores = new GenerationScore(networks.Count);

            for (int gen = 0; gen < ViewModel.DesiredGenerationCount; gen++)
            {
                for (int blue = 0; blue < networks.Count; blue++)
                {
                    for (int red = 0; red < networks.Count; red++)
                    {
                        TrainingGame game = new TrainingGame(networks[blue], networks[red]);
                        var score = game.Play();
                        scores.Score(blue, red, score.Item1, score.Item2);
                        worker.ReportProgress(1);
                    }
                }

                int tenth = networks.Count / 10;
                int childrenCount = (networks.Count - tenth) / tenth;

                int[] best = scores.GetBestN(tenth);
                List<NeuralNetwork> newGeneration = new List<NeuralNetwork>();
                for (int i = 0; i < best.Length; i++)
                {
                    NeuralNetwork oldN = networks[best[i]];
                    newGeneration.Add(oldN);
                    for (int j = 0; j < childrenCount; j++)
                    {
                        NeuralNetwork child = oldN.Clone();
                        child.Mutate(ViewModel.MutationChance);
                        newGeneration.Add(child);
                    }
                }

                evolution.Add(networks);
                networks = newGeneration;
                scores = new GenerationScore(networks.Count);
            }

            e.Result = new Tuple<List<NeuralNetwork>, GenerationScore>(networks, scores);
        }

        private void openNetworksFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Neural Network Items|*.nni|Neural Networks|*.nn|All supported items|*.nni;*.nn";
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                networkFile = (FileStream)dialog.OpenFile();
            }
        }
    }

    public class TrainingGame
    {
        private Player CurrentPlayer;
        private Player OtherPlayer => (Player)((int)CurrentPlayer * -1);

        public TrainingGame(NeuralNetwork player1, NeuralNetwork player2)
        {
            CurrentPlayer = Player.Blue;
        }

        public Tuple<uint, uint> Play()
        {
            throw new NotImplementedException();
        }
    }

    public class GenerationScore
    {
        private readonly Tuple<uint, uint>[,] scores;
        private int NetworkCount => scores.GetLength(0);

        public GenerationScore(int networkCount)
        {
            scores = new Tuple<uint, uint>[networkCount, networkCount];
            for (int i = 0; i < networkCount; i++)
            {
                for (int j = 0; j < networkCount; j++)
                {
                    scores[i, j] = null;
                }
            }
        }

        public bool Score(int network1, int network2, uint score1, uint score2, bool overwrite = false)
        {
            if (scores[network1, network2] == null || overwrite)
            {
                scores[network1, network2] = new Tuple<uint, uint>(score1, score2);
                return true;
            }
            else return false;
        }

        public Tuple<uint, uint> GetScore(int network1, int network2) => scores[network1, network2];

        public uint GetTotalScore(int network)
        {
            uint total = 0;
            for (int i = 0; i < NetworkCount; i++)
            {
                total += scores[network, i].Item1;
                total += scores[i, network].Item2;
            }
            return total;
        }

        public int GetBestIndex()
        {
            int recordIndex = -1;
            uint record = 0;
            for (int i = 0; i < NetworkCount; i++)
            {
                uint score = GetTotalScore(i);
                if (score > record)
                {
                    record = score;
                    recordIndex = i;
                }
            }
            return recordIndex;
        }

        public int[] OrderByScore() => GetBestN(NetworkCount);

        public int[] GetBestN(int n)
        {
            int[] best = new int[n];
            List<Tuple<uint, int>> scores = new List<Tuple<uint, int>>();

            Parallel.For(0, NetworkCount, (i) => scores.Add(new Tuple<uint, int>(GetTotalScore(i), i)));

            for (int i = 0; i < n; i++)
            {
                int recordIndex = -1;
                uint record = 0;
                for (int j = 0; j < NetworkCount - n; j++)
                {
                    if (scores[j].Item1 > record)
                    {
                        recordIndex = j;
                        record = scores[j].Item1;
                    }
                }
                best[n] = scores[recordIndex].Item2;
                scores.Swap(recordIndex, NetworkCount - n - 1);
            }

            return best;
        }
    }

    public static class ArrayExtensions
    {
        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            T temp = array[index1];
            array[index1] = array[index2];
            array[index2] = array[index1];
        }

        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = list[index1];
        }
    }
}
