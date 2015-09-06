using NumberChainSolver.Core;

namespace NumberChainSolver.Validators
{
    public interface INumberChainValidator
    {
        bool IsValid(NumberChain numberChain);
    }
}