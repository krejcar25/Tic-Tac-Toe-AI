using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tic_Tac_Toe_AI
{
    /// <summary>
    /// Interaction logic for SingleGame.xaml
    /// </summary>
    public partial class SingleGameWindow : Window, INotifyPropertyChanged
    {
        Button[,] GameButtons = new Button[20, 20];

        Dictionary<Player, ControlTemplate> ButtonStyles = new Dictionary<Player, ControlTemplate>();

        public event LetAIPlayEventHandler LetAIPlay;

        public int ThreeRowScore { get; set; } = 1;
        public int FourRowScore { get; set; } = 10;
        public int FiveRowScore { get; set; } = 100;

        public int GameSizeX => GameGrid.ColumnDefinitions.Count;
        public int GameSizeY => GameGrid.RowDefinitions.Count - 1;
        public double CellSizeX => GameGrid.ActualWidth / GameSizeX;
        public double CellSizeY => (GameGrid.ActualHeight - PlayerIndicatorButton.ActualHeight) / GameSizeY;
        public Player OtherPlayer => (Player)(((int)CurrentPlayer) * -1);

        private Player _currentPlayer;
        public Player CurrentPlayer
        {
            get => _currentPlayer;
            set
            {
                _currentPlayer = value;
                PlayerIndicatorButton.Template = ButtonStyles[CurrentPlayer];
            }
        }
        private int _scoreRed = 0;
        public int ScoreRed
        {
            get => _scoreRed;
            set
            {
                _scoreRed = value;
                RedPlayerScoreTextBlock.Text = value.ToString();
            }
        }
        private int _scoreBlue = 0;
        public int ScoreBlue
        {
            get => _scoreBlue;
            set
            {
                _scoreBlue = value;
                BluePlayerScoreTextBlock.Text = value.ToString();
            }
        }

        private bool _gameIsPlaying;
        public bool GameIsPlaying
        {
            get => _gameIsPlaying;
            set
            {
                _gameIsPlaying = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameIsPlaying"));
            }
        }

        Dictionary<Player, IArtificialPlayer> ArtificialPlayers = new Dictionary<Player, IArtificialPlayer>();

        public GameLog Log { get; set; }
        public List<GameLog> FinishedLogs { get; private set; }

        public int Move { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public SingleGameWindow(Player first)
        {
            InitializeComponent();

            InitializeGame(first);

            Title += " - PvP";
        }

        public SingleGameWindow(Player first, NeuralNetwork neuralNetwork1, NeuralNetwork neuralNetwork2 = null)
        {
            InitializeComponent();

            InitializeGame(first);

            CheckNetworkDimensions(neuralNetwork1);
            CheckNetworkDimensions(neuralNetwork2);

            ArtificialPlayers[CurrentPlayer] = neuralNetwork1;
            ArtificialPlayers[OtherPlayer] = neuralNetwork2;

            Title += (neuralNetwork2 == null) ? " - PvAI" : " - AIvAI";
        }

        public SingleGameWindow(GameLog gameLog)
        {
            InitializeComponent();

            InitializeGame(gameLog.FirstPlayer);

            CheckNetworkDimensions(gameLog);
            CheckNetworkDimensions(gameLog);

            ArtificialPlayers[CurrentPlayer] = gameLog;
            ArtificialPlayers[OtherPlayer] = gameLog;

            Title += " - Replay";
        }

        public void CheckNetworkDimensions(IArtificialPlayer network)
        {
            if (network == null) return;
            if (!(network.CheckInputNeuronCount(GameSizeX * GameSizeY) && network.CheckOutputNeuronCount(GameSizeX * GameSizeY)))
            {
                throw new ArgumentException(string.Format("Network has bad dimensions. Required dimensions are: {0} input, 2 output", GameSizeX * GameSizeY));
            }
        }

        private void SingleGame_LetAIPlay(object sender, LetAIPlayEventArgs e)
        {
            PlayByMatrix(ArtificialPlayers[CurrentPlayer]?.Predict(GetCurrentGameField(), Move));
        }

        public void PlayByMatrix(DoubleMatrix mx)
        {
            if (mx == null) return;/*
            double xD = mx[0, 0].Map(-1, 1, 0, GameSizeX);
            double yD = mx[1, 0].Map(-1, 1, 0, GameSizeY);

            int x = (int)Math.Floor(xD);
            int y = (int)Math.Floor(yD);*/

            //DoubleMatrix playing = new DoubleMatrix(GameSizeY, GameSizeX, MatrixInitMode.Null);

            double record = double.MinValue;
            int recX = -1;
            int recY = -1;

            for (int y = 0; y < GameSizeY; y++)
            {
                for (int x = 0; x < GameSizeX; x++)
                {
                    double current = mx[y * GameSizeX + x, 0];
                    if (current > record && !((GameButtonProperties)GameButtons[x, y].Tag).Taken)
                    {
                        recX = x;
                        recY = y;
                        record = current;
                    }
                }
            }
            try
            {
                bool played = PlaySelected(GameButtons[recX, recY]);
                if (!played) MessageBox.Show("AI can't play now!", "AI Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Game grid is full");
            }

            /*
            if (rotate < 4 && !PlaySelected(GameButtons[x, y])) LetAIPlay?.Invoke(this, new LetAIPlayEventArgs(rotate + 1));
            else if (rotate >= 4) MessageBox.Show("AI can't play now!", "AI Error", MessageBoxButton.OK, MessageBoxImage.Error);*/
        }

        private void InitializeGame(Player first)
        {
            ButtonStyles.Add(Player.None, (ControlTemplate)Resources["GameButtonEmpty"]);
            ButtonStyles.Add(Player.Blue, (ControlTemplate)Resources["GameButtonBlue"]);
            ButtonStyles.Add(Player.Red, (ControlTemplate)Resources["GameButtonRed"]);

            ArtificialPlayers.Add(Player.Blue, null);
            ArtificialPlayers.Add(Player.Red, null);

            CurrentPlayer = first;
            GameIsPlaying = true;

            LetAIPlay += SingleGame_LetAIPlay;
            Move = 0;

            Log = new GameLog(first);
            FinishedLogs = new List<GameLog>();

            Border border = new Border();
            border.SetValue(Grid.ColumnSpanProperty, GameButtons.GetLength(0));
            border.SetValue(Grid.RowSpanProperty, GameButtons.GetLength(1));
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(1);
            border.Background = null;
            GameGrid.Children.Add(border);
            for (int i = 0; i < GameButtons.GetLength(0); i++)
            {
                for (int j = 0; j < GameButtons.GetLength(1); j++)
                {
                    Button b = new Button();

                    b.Template = ButtonStyles[Player.None];
                    b.Tag = new GameButtonProperties(i, j);
                    b.Click += gameButton_Click;

                    b.SetValue(Grid.ColumnProperty, i);
                    b.SetValue(Grid.RowProperty, j);
                    b.SetValue(Panel.ZIndexProperty, 0);

                    b.Foreground = Brushes.White;

                    b.SetBinding(IsEnabledProperty, "GameIsPlaying");

                    GameButtons[i, j] = b;
                    GameGrid.Children.Add(b);
                }
            }

            ScoreRed = 0;
            ScoreBlue = 0;
        }

        public DoubleMatrix GetCurrentGameField()
        {
            DoubleMatrix matrix = new DoubleMatrix(GameSizeX * GameSizeY, 1, MatrixInitMode.Null);

            Button[,] buttons = GameButtons;
            /*
            for (int i = 0; i < rotate; i++)
            {
                buttons = RotateMatrixCounterClockwise(buttons);
            }*/

            for (int i = 0; i < GameSizeY; i++)
            {
                for (int j = 0; j < GameSizeX; j++)
                {
                    matrix[i * GameSizeX + j, 0] = (int)((GameButtonProperties)buttons[i, j].Tag).Owner;
                }
            }
            return matrix;
        }

        public void gameButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PlaySelected(button);
        }

        public bool PlaySelected(Button button)
        {
            if (!GameIsPlaying) return false;
            GameButtonProperties properties = (GameButtonProperties)button.Tag;
            if (!properties.Taken)
            {
                button.Template = ButtonStyles[CurrentPlayer];
                properties.Owner = CurrentPlayer;
                CurrentPlayer = OtherPlayer;
                CalculateScore();
                Move++;
                Log.Push(properties.X, properties.Y);
                LetAIPlay?.Invoke(this, new LetAIPlayEventArgs());
                return true;
            }
            else return false;
        }

        public void CalculateScore()
        {
            int sizeX = GameButtons.GetLength(0);
            int sizeY = GameButtons.GetLength(1);
            int scoreRed = 0;
            int scoreBlue = 0;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    int score = 0;
                    GameButtonProperties properties = (GameButtonProperties)GameButtons[i, j].Tag;
                    if (properties.Taken)
                    {
                        if (i + 1 < sizeX && GetButtonPlayer(i + 1, j) == properties.Owner)
                        {
                            if (i + 2 < sizeX && GetButtonPlayer(i + 2, j) == properties.Owner)
                            {
                                if (i + 3 < sizeX && GetButtonPlayer(i + 3, j) == properties.Owner)
                                {
                                    if (i + 4 < sizeX && GetButtonPlayer(i + 4, j) == properties.Owner)
                                    {
                                        score += FiveRowScore;
                                        score -= FourRowScore;
                                        FoundFive(properties.Owner);
                                    }
                                    else
                                    {
                                        score += FourRowScore;
                                        score -= ThreeRowScore;
                                    }
                                }
                                else
                                {
                                    score += ThreeRowScore;
                                }
                            }
                        }

                        if (i + 1 < sizeX && j + 1 < sizeY && GetButtonPlayer(i + 1, j + 1) == properties.Owner)
                        {
                            if (i + 2 < sizeX && j + 2 < sizeY && GetButtonPlayer(i + 2, j + 2) == properties.Owner)
                            {
                                if (i + 3 < sizeX && j + 3 < sizeY && GetButtonPlayer(i + 3, j + 3) == properties.Owner)
                                {
                                    if (i + 4 < sizeX && j + 4 < sizeY && GetButtonPlayer(i + 4, j + 4) == properties.Owner)
                                    {
                                        score += FiveRowScore;
                                        score -= FourRowScore;
                                        FoundFive(properties.Owner);
                                    }
                                    else
                                    {
                                        score += FourRowScore;
                                        score -= ThreeRowScore;
                                    }
                                }
                                else
                                {
                                    score += ThreeRowScore;
                                }
                            }
                        }

                        if (j + 1 < sizeY && GetButtonPlayer(i, j + 1) == properties.Owner)
                        {
                            if (j + 2 < sizeY && GetButtonPlayer(i, j + 2) == properties.Owner)
                            {
                                if (j + 3 < sizeY && GetButtonPlayer(i, j + 3) == properties.Owner)
                                {
                                    if (j + 4 < sizeY && GetButtonPlayer(i, j + 4) == properties.Owner)
                                    {
                                        score += FiveRowScore;
                                        score -= FourRowScore;
                                        FoundFive(properties.Owner);
                                    }
                                    else
                                    {
                                        score += FourRowScore;
                                        score -= ThreeRowScore;
                                    }
                                }
                                else
                                {
                                    score += ThreeRowScore;
                                }
                            }
                        }

                        if (i - 1 >= 0 && j + 1 < sizeY && GetButtonPlayer(i - 1, j + 1) == properties.Owner)
                        {
                            if (i - 2 >= 0 && j + 2 < sizeY && GetButtonPlayer(i - 2, j + 2) == properties.Owner)
                            {
                                if (i - 3 >= 0 && j + 3 < sizeY && GetButtonPlayer(i - 3, j + 3) == properties.Owner)
                                {
                                    if (i - 4 >= 0 && j + 4 < sizeY && GetButtonPlayer(i - 4, j + 4) == properties.Owner)
                                    {
                                        score += FiveRowScore;
                                        score -= FourRowScore;
                                        FoundFive(properties.Owner);
                                    }
                                    else
                                    {
                                        score += FourRowScore;
                                        score -= ThreeRowScore;
                                    }
                                }
                                else
                                {
                                    score += ThreeRowScore;
                                }
                            }
                        }

                        switch (properties.Owner)
                        {
                            case Player.Red:
                                scoreRed += score;
                                break;
                            case Player.Blue:
                                scoreBlue += score;
                                break;
                            case Player.None:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            ScoreBlue = scoreBlue;
            ScoreRed = scoreRed;
        }

        public void FoundFive(Player winner)
        {
            PlayerIndicatorButton.Template = ButtonStyles[Player.None];
            PlayerIndicatorButton.IsEnabled = true;
            GameIsPlaying = false;
            Log.Seal();
            FinishedLogs.Add(Log);
            MessageBox.Show(string.Format("Player {0} won!", winner.ToString()), "Game over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public Player GetButtonPlayer(int x, int y) => ((GameButtonProperties)GameButtons[x, y].Tag).Owner;

        private void PlayerIndicatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            PlayerIndicatorButton.Template = ButtonStyles[CurrentPlayer];
        }

        public void ResetGame()
        {
            for (int i = 0; i < GameButtons.GetLength(0); i++)
            {
                for (int j = 0; j < GameButtons.GetLength(1); j++)
                {
                    ((GameButtonProperties)GameButtons[i, j].Tag).Owner = Player.None;
                    GameButtons[i, j].Template = ButtonStyles[Player.None];
                }
            }
            GameIsPlaying = true;
            CurrentPlayer = Log.FirstPlayer;
            Log = new GameLog(CurrentPlayer);
            LetAIPlay?.Invoke(this, new LetAIPlayEventArgs());
        }

        public static T[,] RotateMatrixCounterClockwise<T>(T[,] oldMatrix)
        {
            T[,] newMatrix = new T[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
            int newColumn, newRow = 0;
            for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
            {
                newColumn = 0;
                for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
                {
                    newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
                    newColumn++;
                }
                newRow++;
            }
            return newMatrix;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LetAIPlay?.Invoke(this, new LetAIPlayEventArgs());
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "TicTacToeGameLogs";
            dialog.DefaultExt = ".jlog";
            dialog.Filter = "JSON Log Files|*.jlog";
            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                FileStream fs = (FileStream)dialog.OpenFile();
                string json = await Task.Run(() => JsonConvert.SerializeObject(FinishedLogs, Formatting.Indented));
                byte[] jsonB = new UTF8Encoding(true).GetBytes(json);
                fs.Write(jsonB, 0, jsonB.Length);
                fs.Close();
            }
        }
    }

    public delegate void LetAIPlayEventHandler(object sender, LetAIPlayEventArgs e);

    public class LetAIPlayEventArgs : EventArgs
    {
        public int Rotate { get; set; }

        public LetAIPlayEventArgs(int rotate = 0)
        {
            Rotate = rotate;
        }
    }

    public class GameButtonProperties
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Taken => Owner != Player.None;
        public Player Owner { get; set; }

        public GameButtonProperties(int x, int y)
        {
            X = x;
            Y = y;
            Owner = Player.None;
        }
    }

    public enum Player : int { Red = -1, Blue = 1, None = 0 }

    public static class ExtensionMethods
    {

        public static double Map(this double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }
}
