using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toe_AI
{
    public interface IArtificialPlayer
    {
        DoubleMatrix Predict(DoubleMatrix input, int move);
        bool CheckInputNeuronCount(int desiredCount);
        bool CheckOutputNeuronCount(int desiredCount);
    }
}
