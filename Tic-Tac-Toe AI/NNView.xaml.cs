using System;
using System.Collections.Generic;
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
    /// Interaction logic for NNView.xaml
    /// </summary>
    public partial class NNView : Window
    {
        private double neuronDiameter = 50;
        private double neuronRadius => neuronDiameter / 2;
        private double marginVertical = 10;
        private double marginHorizontal = 75;

        public NNView(NeuralNetwork neuralNetwork)
        {
            InitializeComponent();

            for (int i = 0; i < neuralNetwork.InputNeuronCount; i++)
            {
                double y = i * (neuronDiameter + marginVertical);
                AddNeuron(0, y, Brushes.Green);
                for (int j = 0; j < neuralNetwork.HiddenNeuronsCounts[0]; j++)
                {
                    AddConnection(
                        new Point(neuronRadius, y + neuronRadius),
                        new Point(neuronDiameter + marginHorizontal + neuronRadius, j * (neuronDiameter + marginVertical) + neuronRadius),
                        neuralNetwork["input", i, j]);
                }
            }

            for (int i = 0; i < neuralNetwork.HiddenLayerCount; i++)
            {
                for (int j = 0; j < neuralNetwork.HiddenNeuronsCounts[i]; j++)
                {
                    double x = (i + 1) * (marginHorizontal + neuronDiameter);
                    double y = j * (neuronDiameter + marginVertical);
                    AddNeuron(x, y, Brushes.Yellow, neuralNetwork[i, j]);
                    if (i + 1 < neuralNetwork.HiddenLayerCount)
                    {
                        for (int k = 0; k < neuralNetwork.HiddenNeuronsCounts[i + 1]; k++)
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
                for (int j = 0; j < neuralNetwork.HiddenNeuronsCounts[neuralNetwork.HiddenLayerCount - 1]; j++)
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
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = color;
            ellipse.Stroke = null;
            ellipse.Width = neuronDiameter;
            ellipse.Height = neuronDiameter;
            ellipse.Margin = new Thickness(left, top, 0, 0);
            ellipse.HorizontalAlignment = HorizontalAlignment.Left;
            ellipse.VerticalAlignment = VerticalAlignment.Top;
            ellipse.SetValue(Panel.ZIndexProperty, 0);
            if (bias != null) ellipse.ToolTip = string.Format("Bias: {0}", bias);
            Grid.Children.Add(ellipse);
        }

        private void AddConnection(Point A, Point B, double weight)
        {
            Line line = new Line();
            line.Stroke = Brushes.Blue;
            line.SetValue(Panel.ZIndexProperty, 10);
            line.Fill = null;
            line.StrokeThickness = 1;
            line.X1 = A.X;
            line.Y1 = A.Y;
            line.X2 = B.X;
            line.Y2 = B.Y;
            line.ToolTip = string.Format("Weight: {0}", weight);
            Grid.Children.Add(line);
        }
    }
}
