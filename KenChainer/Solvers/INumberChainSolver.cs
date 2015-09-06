using System.Collections.Generic;
using System.Threading.Tasks;
using KenChainer.Core;

namespace KenChainer.Solvers
{
    public interface INumberChainSolver
    {
        Task<NumberChain> Solve(NumberChainProblem problem);
        Task<IEnumerable<NumberChain>> SolveMany(NumberChainProblem problem);
    }
}