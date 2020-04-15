using System.Threading.Tasks;
using MuTest.Console.Options;
using MuTest.Core.Common;

namespace MuTest.Console
{
    public interface IMuTestRunner
    {
        Task RunMutationTest(MuTestOptions options);

        IMutantExecutor MutantExecutor { get; }
    }
}