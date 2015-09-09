namespace KenChainer.Core
{
    public class NumberChain
    {
        public NumberChain(byte number, NumberChain previous  = null, ArithmeticOperation? operation = null, ushort? result = null)
        {
            Number = number;
            Previous = previous;
            if (Previous != null)
                Operation = operation;
            Result = result;
            if (Result == null && Previous != null && Operation != null)
                Result = Operation.Value.GetResult(Previous.Result ?? Previous.Number, Number);
        }

        public byte Number { get; }
        public ArithmeticOperation? Operation { get; }
        public ushort? Result { get; }
        public NumberChain Previous { get; }
        public byte Length => (byte)(Previous?.Length + 1 ?? 1);
        public override string ToString()
        {
            return Previous + Operation?.GetSign() + Number;
        }
    }
}