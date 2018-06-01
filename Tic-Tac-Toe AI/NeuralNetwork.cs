using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toe_AI
{
    public partial class NeuralNetwork
    {
        // Self-explanatory properties
        /// <summary>
        /// How many imput neurons does this Neural Network have
        /// </summary>
        public int InputNeuronCount => connections[0].Rows;
        /// <summary>
        /// How many output neurons does this Neural Network have
        /// </summary>
        public int OutputNeuronsCount => connections[connections.Count - 1].Rows;
        /// <summary>
        /// How many hidden neurons does this Neural Network have - use indexer
        /// </summary>
        public ReadOnlyDictionary<int, int> HiddenNeuronsCounts
        {
            get
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                for (int i = 1; i < connections.Count; i++)
                {
                    counts.Add(i - 1, connections[i].Rows);
                }
                return new ReadOnlyDictionary<int, int>(counts);
            }
        }
        /// <summary>
        /// How many hidden layers does this Neural Network have
        /// </summary>
        public int HiddenLayerCount => connections.Count - 1;

        // Connection matrices dictionary
        // Next layer's neuron count as rows count, previous layer's as cols count
        private Dictionary<int, DoubleMatrix> connections = new Dictionary<int, DoubleMatrix>();
        
        // Bias matrices, one for each hidden layer and one for output
        private Dictionary<int, DoubleMatrix> biases = new Dictionary<int, DoubleMatrix>();
        private DoubleMatrix outputBias;

        private readonly ActivationFunction activation;
        private readonly ActivationFunction activationD;

        // Main constructor
        public NeuralNetwork(int inputNeuronCount, int outputNeuronCount, ActivationFunctionType activationFunctionType, params int[] hiddenNeuronCounts)
        {
            // Check input validity
            if (inputNeuronCount <= 0) throw new ArgumentException("Input neuron count must be positive, non-zero");
            if (outputNeuronCount <= 0) throw new ArgumentException("Output neuron count must be positive, non-zero");
            if (hiddenNeuronCounts.Contains(0)) throw new ArgumentException("hiddenNeuronsCounts contains zero-length layer, this is illegal");

            // Input ➡ First Hidden
            connections.Add(0, new DoubleMatrix(hiddenNeuronCounts[0], inputNeuronCount, MatrixInitMode.RanNorm));
            int i;
            for (i = 1; i < hiddenNeuronCounts.Length; i++)
            {
                // i-1'th ➡ i'th hidden layer connection matrix
                connections.Add(i, new DoubleMatrix(hiddenNeuronCounts[i], hiddenNeuronCounts[i - 1], MatrixInitMode.RanNorm));
                // i'th layer bias
                biases.Add(i, new DoubleMatrix(hiddenNeuronCounts[i], 1, MatrixInitMode.RanNorm));
            }
            // Last hidden ➡ output layer connection
            connections.Add(i, new DoubleMatrix(outputNeuronCount, hiddenNeuronCounts[i - 1], MatrixInitMode.RanNorm));
            // Output bias
            outputBias = new DoubleMatrix(outputNeuronCount, 1, MatrixInitMode.RanNorm);

            // Set activation function
            switch (activationFunctionType)
            {
                case ActivationFunctionType.Sigmoid:
                    activation = SigmoidActivationFunction;
                    activationD = SigmoidActivationFunctionDerivative;
                    break;
                case ActivationFunctionType.HyperbolicTangent:
                    activation = HyperbolicTangentActivationFunction;
                    activationD = HyperbolicTangentActivationFunctionDerivative;
                    break;
                default:
                    break;
            }
        }
        
        /// <summary>
        /// Predicts an answer based on current weights and biases. Call this to get actual results according to current state of this Neural Network
        /// </summary>
        /// <param name="input">Input values, must have the same number of rows as input neurons</param>
        /// <returns>A DoubleMatrix that represents outputs from this Neural Network</returns>
        public DoubleMatrix Predict(DoubleMatrix input)
        {
            List<DoubleMatrix> hiddenLayers;
            return Predict(input, out hiddenLayers);
        }

        private DoubleMatrix Predict(DoubleMatrix input, out List<DoubleMatrix> hiddenLeyers)
        {
            // Check input validity
            if (input.Rows != InputNeuronCount) throw new ArgumentException("Input matrix must have the same number of rows as number of input neurons");
            hiddenLeyers = new List<DoubleMatrix>();

            // Pass values from input to first hidden layer
            DoubleMatrix intermediate = connections[0] * input;
            // Add that layer's biases
            intermediate += biases[0];
            // and activate the values (normalise them)
            intermediate.Activate(activation);
            hiddenLeyers.Add(intermediate);

            int i;
            for (i = 1; i < HiddenLayerCount; i++)
            {
                // Calculate the next layer (i'th), add the biases of that layer and normalise values
                // Also this loop wont even run if we only have one hidden layer
                intermediate = connections[i] * intermediate;
                intermediate += biases[i];
                intermediate.Activate(activation);
                hiddenLeyers.Add(intermediate);
            }

            // Calculate the output layer, add output biases and normalise values
            intermediate = connections[i] * intermediate;
            intermediate += outputBias;
            intermediate.Activate(activation);
            hiddenLeyers.Add(intermediate);

            // Done
            return intermediate;
        }

        // Not working yet
        public void Learn(DoubleMatrix input, DoubleMatrix desiredOutput)
        {
            List<DoubleMatrix> hiddenLayers;
            DoubleMatrix result = Predict(input, out hiddenLayers);
            DoubleMatrix outputError = desiredOutput - result;
        }

        #region Activation Functions
        // Some mathsy shenanigans
        public static double SigmoidActivationFunction(double value) => 1.0 / (1.0 + Math.Exp(-value));

        public static double SigmoidActivationFunctionDerivative(double value) => value * (1 - value);

        public static double HyperbolicTangentActivationFunction(double value) => Math.Tanh(value);

        public static double HyperbolicTangentActivationFunctionDerivative(double value) => 1 / Math.Pow(Math.Cosh(value), 2);
        #endregion
    }

    public delegate double ActivationFunction(double value);
    public enum ActivationFunctionType { Sigmoid, HyperbolicTangent };
}
