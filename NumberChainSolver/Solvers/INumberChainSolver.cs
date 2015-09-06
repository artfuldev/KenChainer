using System.Collections.Generic;
using System.Threading.Tasks;
using NumberChainSolver.Core;

namespace NumberChainSolver.Solvers
{
    public interface INumberChainSolver
    {
        Task<NumberChain> Solve(NumberChainProblem problem);
        Task<IEnumerable<NumberChain>> SolveMany(NumberChainProblem problem);
    }
}