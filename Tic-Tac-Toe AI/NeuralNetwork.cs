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
        public int InputNeuronCount => connections[0].Rows;
        public int OutputNeuronsCount => connections[connections.Count - 1].Rows;
        public ReadOnlyDictionary<int, int> HiddenNeuronsCounts
        {
            get
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                for (int i = 1; i < connections.Count; i++)
                {
                    counts.Add(i, connections[i].Rows);
                }
                return new ReadOnlyDictionary<int, int>(counts);
            }
        }

        Dictionary<int, DoubleMatrix> connections = new Dictionary<int, DoubleMatrix>();
        Dictionary<int, DoubleMatrix> biases = new Dictionary<int, DoubleMatrix>();
        DoubleMatrix outputBias;
        public NeuralNetwork(int inputNeuronCount, int outputNeuronCount, params int[] hiddenNeuronCounts)
        {
            if (inputNeuronCount <= 0) throw new ArgumentException("Input neuron count must be positive, non-zero");
            if (outputNeuronCount <= 0) throw new ArgumentException("Output neuron count must be positive, non-zero");
            if (hiddenNeuronCounts.Contains(0)) throw new ArgumentException("hiddenNeuronsCounts contains zero-length layer, this is illegal");

            connections.Add(0, new DoubleMatrix(hiddenNeuronCounts[0], inputNeuronCount, MatrixInitMode.RanNorm));
            int i;
            for (i = 1; i < hiddenNeuronCounts.Length; i++)
            {
                connections.Add(i, new DoubleMatrix(hiddenNeuronCounts[i], hiddenNeuronCounts[i - 1], MatrixInitMode.RanNorm));
                biases.Add(i, new DoubleMatrix(hiddenNeuronCounts[i], hiddenNeuronCounts[i - 1], MatrixInitMode.RanNorm));
            }
            connections.Add(i, new DoubleMatrix(hiddenNeuronCounts[i - 1], outputNeuronCount, MatrixInitMode.RanNorm));

            outputBias = new DoubleMatrix(outputNeuronCount, 1, MatrixInitMode.RanNorm);
        }

        public DoubleMatrix Predict(DoubleMatrix input)
        {
            if (input.Rows != InputNeuronCount) throw new ArgumentException("Input matrix must have the same number of rows as number of input neurons");
        }
    }
}
