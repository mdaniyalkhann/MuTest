using MuTest.Cpp.CLI.Model;

namespace MuTest.Cpp.CLI.Core
{
    public interface ICppDirectoryFactory
    {
        int NumberOfMutantsExecutingInParallel { get; set; }

        CppBuildContext PrepareTestDirectories(string testClass, string sourceClass, string testProject, string testSolution);
    }
}