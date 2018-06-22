using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toe_AI
{
    public class GameLog : IArtificialPlayer
    {
        [JsonProperty]
        private List<Tuple<int, int>> Moves;
        private bool Finished = false;
        public Player FirstPlayer { get; set; }

        public GameLog(Player firstPlayer)
        {
            FirstPlayer = firstPlayer;
            Moves = new List<Tuple<int, int>>();
        }

        public bool Push(int x, int y)
        {
            if (!Finished) Moves.Add(new Tuple<int, int>(x, y));
            return !Finished;
        }

        public void Seal()
        {
            Finished = true;
        }

        public bool CheckInputNeuronCount(int desiredCount) => true;
        public bool CheckOutputNeuronCount(int desiredCount) => true;

        public DoubleMatrix Predict(DoubleMatrix input, int move)
        {
            DoubleMatrix matrix =  new DoubleMatrix(2, 1, MatrixInitMode.Null);
            matrix[0, 0] = Moves[move].Item1;
            matrix[1, 0] = Moves[move].Item2;
            return matrix;
        }
    }
}
