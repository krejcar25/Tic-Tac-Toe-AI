using Newtonsoft.Json;
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
        [JsonIgnore]
        public int InputNeuronCount => connections[0].Cols;
        /// <summary>
        /// How many output neurons does this Neural Network have
        /// </summary>
        [JsonIgnore]
        public int OutputNeuronCount => connections[connections.Count - 1].Rows;
        /// <summary>
        /// How many hidden neurons does this Neural Network have - use indexer
        /// </summary>
        [JsonIgnore]
        public ReadOnlyDictionary<int, int> HiddenNeuronCounts
        {
            get
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                for (int i = 1; i < connections.Count; i++)
                {
                    counts.Add(i - 1, connections[i].Cols);
                }
                return new ReadOnlyDictionary<int, int>(counts);
            }
        }
        /// <summary>
        /// How many hidden layers does this Neural Network have
        /// </summary>
        [JsonIgnore]
        public int HiddenLayerCount => connections.Count - 1;

        // Connection matrices dictionary
        // Next layer's neuron count as rows count, previous layer's as cols count
        [JsonProperty]
        private Dictionary<int, DoubleMatrix> connections = new Dictionary<int, DoubleMatrix>();

        // Bias matrices, one for each hidden layer and one for output
        [JsonProperty]
        private Dictionary<int, DoubleMatrix> biases = new Dictionary<int, DoubleMatrix>();
        [JsonProperty]
        private DoubleMatrix outputBias;

        public double this[string layer, int indexFrom, int indexTo]
        {
            get
            {
                if (layer == "input")
                {
                    if (indexFrom >= InputNeuronCount) throw new IndexOutOfRangeException(string.Format("indexFrom must be in range of input layer's neuron count ({0})", InputNeuronCount));
                    else if (indexTo >= HiddenNeuronCounts[0]) throw new IndexOutOfRangeException(string.Format("indexTo must be in range of 1st hidden layer's neuron count ({0})", HiddenNeuronCounts[0]));
                    else return connections[0][indexTo, indexFrom];
                }
                else if (layer == "output")
                {
                    if (indexFrom >= HiddenNeuronCounts[HiddenLayerCount - 1]) throw new IndexOutOfRangeException(string.Format("indexFrom must be in range of last hidden layer's neuron count ({0})", HiddenNeuronCounts[HiddenLayerCount - 1]));
                    else if (indexTo >= OutputNeuronCount) throw new IndexOutOfRangeException(string.Format("indexTo must be in range of 1st hidden layer's neuron count ({0})", OutputNeuronCount));
                    else return connections[connections.Count - 1][indexTo, indexFrom];
                }
                else if (int.TryParse(layer, out int layerIndex)) return this[layerIndex, indexFrom, indexTo];
                else throw new ArgumentException("Layer name unknown", "layer");
            }
        }

        public double this[int layer, int indexFrom, int indexTo]
        {
            get
            {
                if (layer >= HiddenLayerCount) throw new IndexOutOfRangeException(string.Format("layer must be in range of HiddenLayerCount ({0})", HiddenLayerCount));
                else if (indexFrom >= HiddenNeuronCounts[layer]) throw new IndexOutOfRangeException(string.Format("indexFrom must be in range of layer {0}'s neuron count ({1})", layer, HiddenNeuronCounts[layer]));
                else if (indexTo >= HiddenNeuronCounts[layer + 1]) throw new IndexOutOfRangeException(string.Format("indexTo must be in range of layer {0}'s neuron count ({1})", layer + 1, HiddenNeuronCounts[layer + 1]));
                else return connections[layer + 1][indexTo, indexFrom];
            }
        }

        public double this[int layer, int index]
        {
            get
            {
                if (layer >= biases.Count) throw new IndexOutOfRangeException(string.Format("layer must be in range of biases Count ({0})", biases.Count));
                else if (index >= biases[layer].Rows) throw new IndexOutOfRangeException(string.Format("index must be in range of layer {0}'s neuron count ({1})", layer, biases[layer].Rows));
                return biases[layer][index, 0];
            }
        }

        public double this[int index]
        {
            get
            {
                if (index >= outputBias.Rows) throw new IndexOutOfRangeException(string.Format("index must be in range of output layers neuron count ({0})", outputBias.Rows));
                else return outputBias[index, 0];
            }
        }

        private readonly ActivationFunction activation;
        private readonly ActivationFunction activationD;

        public ActivationFunctionType ActivationFunctionType { get; set; }

        public double LearningRate { get; private set; }

        // Main constructor
        public NeuralNetwork(int inputNeuronCount, int outputNeuronCount, ActivationFunctionType activationFunctionType, params int[] hiddenNeuronCounts)
        {
            // Check input validity
            if (inputNeuronCount <= 0) throw new ArgumentException("Input neuron count must be positive, non-zero");
            if (outputNeuronCount <= 0) throw new ArgumentException("Output neuron count must be positive, non-zero");
            if (hiddenNeuronCounts.Contains(0)) throw new ArgumentException("hiddenNeuronsCounts contains zero-length layer, this is illegal");

            ActivationFunctionType = activationFunctionType;

            // Input ➡ First Hidden
            connections.Add(0, new DoubleMatrix(hiddenNeuronCounts[0], inputNeuronCount, MatrixInitMode.RanNorm));
            biases.Add(0, new DoubleMatrix(hiddenNeuronCounts[0], 1, MatrixInitMode.RanNorm));
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

            LearningRate = 0.1;
        }

        [JsonConstructor]
        public NeuralNetwork(Dictionary<int,DoubleMatrix> connections, Dictionary<int,DoubleMatrix> biases, DoubleMatrix outputBias, double learningRate, ActivationFunctionType activationFunctionType)
        {
            this.connections = connections;
            this.biases = biases;
            this.outputBias = outputBias;
            LearningRate = learningRate;
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
            return Predict(input, out List<DoubleMatrix> hiddenLayers);
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

        /*
        // Not working yet, doesn't need to
        public void Learn(DoubleMatrix input, DoubleMatrix desiredOutput)
        {
            // throw new NotImplementedException("Nope! Bad!");
            // Generate output
            DoubleMatrix result = Predict(input, out List<DoubleMatrix> hiddenLayers);
            
            // Calculate error as ERROR = DESIRED - ACTUAL
            DoubleMatrix outputError = desiredOutput - result;

            // Calculate gradient
            DoubleMatrix outputGradient = result.Clone();
            outputGradient.Activate(activationD);
            outputGradient = outputError * outputGradient;
            outputGradient *= LearningRate;


        }*/

        #region Activation Functions
        // Some mathsy shenanigans
        public static double SigmoidActivationFunction(double value) => 1.0 / (1.0 + Math.Exp(-value));

        public static double SigmoidActivationFunctionDerivative(double value) => value * (1 - value);

        public static double HyperbolicTangentActivationFunction(double value) => Math.Tanh(value);

        public static double HyperbolicTangentActivationFunctionDerivative(double value) => 1 / Math.Pow(Math.Cosh(value), 2);
        #endregion

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public delegate double ActivationFunction(double value);
    public enum ActivationFunctionType { Sigmoid, HyperbolicTangent };
}
