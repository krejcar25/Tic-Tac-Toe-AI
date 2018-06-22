using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Tic_Tac_Toe_AI
{
    /// <summary>
    /// Interaction logic for NNView.xaml
    /// </summary>
    public partial class NNView : Window
    {
        private double neuronDiameter = 50;
        private double neuronRadius => neuronDiameter / 2;
        private double marginVertical = 75;
        private double marginHorizontal = 75;
        private double connectionThickness = 8;

        public NNView(NeuralNetwork neuralNetwork)
        {
            InitializeComponent();

            for (int i = 0; i < neuralNetwork.InputNeuronCount; i++)
            {
                double y = i * (neuronDiameter + marginVertical);
                AddNeuron(0, y, Brushes.Green);
                for (int j = 0; j < neuralNetwork.HiddenNeuronCounts[0]; j++)
                {
                    AddConnection(
                        new Point(neuronRadius, y + neuronRadius),
                        new Point(neuronDiameter + marginHorizontal + neuronRadius, j * (neuronDiameter + marginVertical) + neuronRadius),
                        neuralNetwork["input", i, j]);
                }
            }

            for (int i = 0; i < neuralNetwork.HiddenLayerCount; i++)
            {
                for (int j = 0; j < neuralNetwork.HiddenNeuronCounts[i]; j++)
                {
                    double x = (i + 1) * (marginHorizontal + neuronDiameter);
                    double y = j * (neuronDiameter + marginVertical);
                    AddNeuron(x, y, Brushes.Yellow, neuralNetwork[i, j]);
                    if (i + 1 < neuralNetwork.HiddenLayerCount)
                    {
                        for (int k = 0; k < neuralNetwork.HiddenNeuronCounts[i + 1]; k++)
                        {
                            AddConnection(
                                new Point(x + neuronRadius, y + neuronRadius),
                                new Point((i + 2) * (marginHorizontal + neuronDiameter) + neuronRadius, k * (neuronDiameter + marginVertical) + neuronRadius),
                                neuralNetwork[i, j, k]);
                        }
                    }
                }
            }

            for (int i = 0; i < neuralNetwork.OutputNeuronCount; i++)
            {
                double x = (neuralNetwork.HiddenLayerCount + 1) * (marginHorizontal + neuronDiameter);
                double y = i * (neuronDiameter + marginVertical);
                AddNeuron(x, y, Brushes.Red, neuralNetwork[i]);
                for (int j = 0; j < neuralNetwork.HiddenNeuronCounts[neuralNetwork.HiddenLayerCount - 1]; j++)
                {
                    AddConnection(
                        new Point(x + neuronRadius, y + neuronRadius),
                        new Point(neuralNetwork.HiddenLayerCount * (marginHorizontal + neuronDiameter) + neuronRadius, j * (neuronDiameter + marginVertical) + neuronRadius),
                        neuralNetwork["output", j, i]);
                }
            }
        }

        private void AddNeuron(double left, double top, Brush color, double? bias = null)
        {
            if (bias != null)
            {
                Ellipse background = new Ellipse();
                background.Fill = Brushes.LightGray;
                background.Stroke = null;
                background.Width = neuronDiameter;
                background.Height = neuronDiameter;
                background.Margin = new Thickness(left, top, 0, 0);
                background.HorizontalAlignment = HorizontalAlignment.Left;
                background.VerticalAlignment = VerticalAlignment.Top;
                background.SetValue(Panel.ZIndexProperty, 0);
                if (bias != null) background.ToolTip = string.Format("Bias: {0}", bias);
                Grid.Children.Add(background);
            }

            double actualDiameter = (bias == null) ? neuronDiameter : neuronDiameter * (double)bias;

            Ellipse neuron = new Ellipse();
            neuron.Fill = color;
            neuron.Stroke = null;
            neuron.Width = actualDiameter;
            neuron.Height = actualDiameter;
            neuron.Margin = new Thickness(left + (neuronDiameter - actualDiameter) / 2, top + (neuronDiameter - actualDiameter) / 2, 0, 0);
            neuron.HorizontalAlignment = HorizontalAlignment.Left;
            neuron.VerticalAlignment = VerticalAlignment.Top;
            neuron.SetValue(Panel.ZIndexProperty, 5);
            if (bias != null) neuron.ToolTip = string.Format("Bias: {0}", bias);
            Grid.Children.Add(neuron);
        }

        private void AddConnection(Point A, Point B, double weight)
        {
            Line background = new Line();
            background.Stroke = Brushes.LightGray;
            background.SetValue(Panel.ZIndexProperty, 0);
            background.Fill = null;
            background.StrokeThickness = connectionThickness;
            background.X1 = A.X;
            background.Y1 = A.Y;
            background.X2 = B.X;
            background.Y2 = B.Y;
            background.ToolTip = string.Format("Weight: {0}", weight);
            Grid.Children.Add(background);

            Line connection = new Line();
            connection.Stroke = Brushes.Blue;
            connection.SetValue(Panel.ZIndexProperty, 10);
            connection.Fill = null;
            connection.StrokeThickness = connectionThickness * weight;
            connection.X1 = A.X;
            connection.Y1 = A.Y;
            connection.X2 = B.X;
            connection.Y2 = B.Y;
            connection.ToolTip = string.Format("Weight: {0}", weight);
            Grid.Children.Add(connection);
        }
    }
}
