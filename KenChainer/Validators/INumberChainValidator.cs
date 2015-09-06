using KenChainer.Core;

namespace KenChainer.Validators
{
    public interface INumberChainValidator
    {
        bool IsValid(NumberChain numberChain);
    }
}