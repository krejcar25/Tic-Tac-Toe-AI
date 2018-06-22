namespace Tic_Tac_Toe_AI
{
    public interface IArtificialPlayer
    {
        DoubleMatrix Predict(DoubleMatrix input, int move);
        bool CheckInputNeuronCount(int desiredCount);
        bool CheckOutputNeuronCount(int desiredCount);
    }
}
