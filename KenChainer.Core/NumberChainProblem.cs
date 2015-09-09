namespace KenChainer.Core
{
    public class NumberChainProblem
    {
        public NumberChainProblem(ushort result, params byte[] numbers)
        {
            Numbers = numbers;
            Result = result;
        }
        public byte[] Numbers { get; private set; }
        public ushort Result { get; private set; }
    }
}