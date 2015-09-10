using System.IO;
using KenChainer.Core;

namespace KenChainer.Transformations
{
    public interface IProblemExtractor
    {
        NumberChainProblem GetProblem(string imagePath);
    }
}