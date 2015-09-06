namespace NumberChainSolver.Core
{
    public class NumberChainProblem
    {
        public NumberChainProblem(ushort result, params ushort[] numbers)
        {
            Numbers = numbers;
            Result = result;
        }
        public ushort[] Numbers { get; private set; }
        public ushort Result { get; private set; }
    }
}