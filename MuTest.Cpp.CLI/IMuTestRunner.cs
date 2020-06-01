using System.Threading.Tasks;
using MuTest.Cpp.CLI.Core;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Options;

namespace MuTest.Cpp.CLI
{
    public interface IMuTestRunner
    {
        Task RunMutationTest(MuTestOptions options);

        ICppMutantExecutor MutantsExecutor { get; }

        ICppDirectoryFactory DirectoryFactory { get; }

        CppBuildContext Context { get; }
    }
}