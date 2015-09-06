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

        public static ushort GetResult(this ArithmeticOperation arithmeticOperation, ushort left, ushort right)
        {
            switch (arithmeticOperation)
            {
                case ArithmeticOperation.Addition:
                    return (ushort)(left + right);
                case ArithmeticOperation.Subtraction:
                    if (left < right) throw new NotSupportedException();
                    return (ushort)(left - right);
                case ArithmeticOperation.Multiplication:
                    return (ushort)(left * right);
                case ArithmeticOperation.Division:
                    if (left%right != 0) throw new NotSupportedException();
                    return (ushort)(left / right);
                case ArithmeticOperation.None:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(arithmeticOperation), arithmeticOperation, null);
            }
        }

        public static bool IsValidOn(this ArithmeticOperation operation, ushort left, ushort right)
        {
            try
            {
                operation.GetResult(left, right);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidOn(this ArithmeticOperation operation, NumberChain left, ushort right)
        {
            return operation.IsValidOn(left.Result ?? left.Number, right);
        }
    }
}