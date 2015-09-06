using System.Collections.Generic;

namespace NumberChainSolver.Core
{
    public static class NumberChainExtensions
    {
        public static ArithmeticOperation[] GetUsedOperations(this NumberChain chain)
        {
            var usedOperations = new List<ArithmeticOperation>();
            while (chain.Operation != null)
            {
                usedOperations.Add(chain.Operation.Value);
                chain = chain.Previous;
            }
            return usedOperations.ToArray();
        } 
    }
}