using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KenChainer.Core;
using KenChainer.Validators;

namespace KenChainer.Solvers
{
    public class BruteForceSolver : INumberChainSolver
    {
        private readonly INumberChainValidator _validator;

        public BruteForceSolver() : this(new NumberChainValidator())
        {
        }

        public BruteForceSolver(INumberChainValidator validator)
        {
            _validator = validator;
        }

        public ArithmeticOperation[] GetPossibleOperations()
        {
            return Enum.GetValues(typeof(ArithmeticOperation))
                .Cast<ArithmeticOperation>()
                .Where(x => x != ArithmeticOperation.None).ToArray();
        }

        public async Task<NumberChain> Solve(NumberChainProblem problem)
        {
            var result = problem.Result;
            var numbers = problem.Numbers;
            if (numbers.Length < 2)
                throw new NotSupportedException("A minimum of two numbers required to operate on");
            var chains = GenerateChains(numbers);
            var solution = chains.FirstOrDefault(x => x.Result == result && _validator.IsValid(x));
            if (solution == null)
                throw new NotFiniteNumberException("No result possible");
            return await Task.FromResult(solution);
        }

        public async Task<IEnumerable<NumberChain>> SolveMany(NumberChainProblem problem)
        {
            var result = problem.Result;
            var numbers = problem.Numbers;
            if (numbers.Length < 2)
                throw new NotSupportedException("A minimum of two numbers required to operate on");
            var chains = GenerateChains(numbers);
            var solutions = chains.Where(x => x.Result == result && _validator.IsValid(x)).ToList();
            if (solutions == null || !solutions.Any())
                throw new NotFiniteNumberException("No result possible");
            return await Task.FromResult(solutions);
        }

        public IEnumerable<NumberChain> GenerateChains(byte[] numbers)
        {
            IEnumerable<NumberChain> chains = new List<NumberChain> { new NumberChain(numbers[0]) };
            var rest = numbers.ToList();
            rest.RemoveAt(0);
            return rest.Aggregate(chains, (current, number) => current.SelectMany(x => GenerateChains(number, x)));
        }

        public IEnumerable<NumberChain> GenerateChains(byte number, NumberChain previous)
        {
            return GetPossibleOperations()
                .Where(x => x.IsValidOn(previous, number))
                .Select(x => new NumberChain(number, previous, x));
        }
    }
}