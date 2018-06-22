using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tic_Tac_Toe_AI
{
    public class AITrainingHubViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _generationProgressBarMaximum;
        public double GenerationProgressBarMaximum
        {
            get => _generationProgressBarMaximum; set
            {
                _generationProgressBarMaximum = value;
                NotifyPropertyChanged("GenerationProgressBarMaximum");
            }
        }

        private double _overallProgressBarMaximum;
        public double OverallProgressBarMaximum
        {
            get => _overallProgressBarMaximum; set
            {
                _overallProgressBarMaximum = value;
                NotifyPropertyChanged("OverallProgressBarMaximum");
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                NotifyPropertyChanged("IsPaused");
                NotifyPropertyChanged("PauseButtonLabel");
            }
        }

        private bool _isChartRadioChacked;
        public bool IsChartRadioChecked
        {
            get => _isChartRadioChacked;
            set
            {
                _isChartRadioChacked = value;
                NotifyPropertyChanged("IsChartRadioChecked");
            }
        }

        private bool _isPairRadioChacked;
        public bool IsPairRadioChecked
        {
            get => _isPairRadioChacked;
            set
            {
                _isPairRadioChacked = value;
                NotifyPropertyChanged("IsPairRadioChecked");
            }
        }

        private string _desiredGenerationCountString;
        public string DesiredGenerationCountString
        {
            get => _desiredGenerationCountString;
            set
            {
                _desiredGenerationCountString = value;
                NotifyPropertyChanged("DesiredGenerationCountString");
            }
        }

        private double _currentProgressGeneration;
        public double CurrentProgressGeneration
        {
            get => _currentProgressGeneration;
            set
            {
                _currentProgressGeneration = value;
                NotifyPropertyChanged("CurrentProgressGeneration");
            }
        }

        private double _currentProgressOverall;
        public double CurrentProgressOverall
        {
            get => _currentProgressOverall;
            set
            {
                _currentProgressOverall = value;
                NotifyPropertyChanged("CurrentProgressOverall");
            }
        }

        private int _gameSizeX;
        public int GameSizeX
        {
            get => _gameSizeX;
            set
            {
                _gameSizeX = value;
                NotifyPropertyChanged("GameSizeX");
            }
        }

        private int _gameSizeY;
        public int GameSizeY
        {
            get => _gameSizeY;
            set
            {
                _gameSizeY = value;
                NotifyPropertyChanged("GameSizeY");
            }
        }

        private string _mutationChanceString;
        public string MutationChanceString
        {
            get => _mutationChanceString;
            set
            {
                _mutationChanceString = value;
                NotifyPropertyChanged("MutationChanceString");
            }
        }

        public bool CancelButtonEnabled => !IsPaused;
        public string PauseButtonLabel => IsPaused ? "Resume" : "Pause";
        public SelectionMode SelectionMode => (IsChartRadioChecked) ? SelectionMode.Chart : ((IsPairRadioChecked) ? SelectionMode.Pair : SelectionMode.None);
        public string CurrentProgressGenerationText => string.Format("{0} %", CurrentProgressGeneration * 100);
        public int DesiredGenerationCount => (int.TryParse(DesiredGenerationCountString, out int count)) ? count : -1;
        public double MutationChance => (double.TryParse(MutationChanceString, out double chance)) ? chance : double.NaN;

        public AITrainingHubViewModel()
        {

        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SelectionMode { None, Chart, Pair }
}
