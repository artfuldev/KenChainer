using System.Linq;
using System.Text.RegularExpressions;
using KenChainer.Core;

namespace KenChainer.Validators
{
    /// <summary>
    /// Default rules for the problem go here.
    /// Currently there are 3 rules:
    /// Only 7 numbers
    /// Only 6 operators
    /// All 4 operators must be used
    /// 2 operators must be used twice
    /// </summary>
    public class NumberChainValidator : INumberChainValidator
    {
        private static readonly Regex Operators = new Regex("\\+|\\-|x|\\/",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        public bool IsValid(NumberChain numberChain)
        {
            try
            {
                // Only seven numbers
                if (numberChain.Length != 7)
                    return false;

                var operators =
                    Operators.Matches(numberChain.ToString()).Cast<object>().Select(x => x.ToString()).ToList();

                // Only 6 operators
                if (operators.Count != 6) return false;

                // All 4 operators should be used
                var grouped = operators.GroupBy(x => x);
                if (grouped.Count() != 4) return false;

                // Maximum of 2 operators can be used twice
                var groupedByCount = grouped.GroupBy(x => x.Count());
                var countOfOperatorsUsedTwice = groupedByCount.FirstOrDefault(x => x.Key == 2)?.Count();
                if (countOfOperatorsUsedTwice != 2)
                    return false;

                // Validated
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
