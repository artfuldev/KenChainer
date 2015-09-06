using System;

namespace KenChainer.Core
{
    public static class ArithmeticOperationExtensions
    {
        public static string GetSign(this ArithmeticOperation arithmeticOperation)
        {
            switch (arithmeticOperation)
            {
                case ArithmeticOperation.Addition:
                    return "+";
                case ArithmeticOperation.Subtraction:
                    return "-";
                case ArithmeticOperation.Multiplication:
                    return "x";
                case ArithmeticOperation.Division:
                    return "/";
                case ArithmeticOperation.None:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(arithmeticOperation), arithmeticOperation, null);
            }
        }

        public static ushort GetResult(this ArithmeticOperation arithmeticOperation, ushort left, byte right)
        {
            if (!arithmeticOperation.IsValidOn(left, right))
                throw new NotSupportedException();
            switch (arithmeticOperation)
            {
                case ArithmeticOperation.Addition:
                    return (ushort)(left + right);
                case ArithmeticOperation.Subtraction:
                    return (ushort)(left - right);
                case ArithmeticOperation.Multiplication:
                    return (ushort)(left * right);
                case ArithmeticOperation.Division:
                    return (ushort)(left / right);
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool IsValidOn(this ArithmeticOperation operation, ushort left, byte right)
        {
            switch (operation)
            {
                case ArithmeticOperation.Addition:
                    return true;
                case ArithmeticOperation.Subtraction:
                    return left > right;
                case ArithmeticOperation.Multiplication:
                    return true;
                case ArithmeticOperation.Division:
                    return left % right == 0;
                default:
                    return false;
            }
        }

        public static bool IsValidOn(this ArithmeticOperation operation, NumberChain left, byte right)
        {
            return operation.IsValidOn(left.Result ?? left.Number, right);
        }
    }
}