using System;
using System.Diagnostics;
using System.Linq;
using NumberChainSolver.Core;
using NumberChainSolver.Solvers;

namespace NumberChainSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            // The Times of India
            // Chennai Times
            // Page 4
            // September 6, 2015
            // Result = 5
            // Sequence = 4_1_9_1_7_6_5
            var problem = new NumberChainProblem(5, 4, 1, 9, 1, 7, 6, 5);
            var solver = new BruteForceSolver();

            // Solve
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var sol = solver.Solve(problem).Result;
            Console.WriteLine("First Solution:\t" + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine(sol.ToString());
            stopwatch.Restart();
            var solutions = solver.SolveMany(problem).Result;
            Console.WriteLine("All Solutions:\t" + stopwatch.ElapsedMilliseconds + "ms");
            foreach (var solution in solutions)
                Console.WriteLine(solution.ToString());
            Console.ReadKey();
        }
    }
}
