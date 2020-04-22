using MuTest.Cpp.CLI.Model;

namespace MuTest.Cpp.CLI.Core
{
    public interface ICppDirectoryFactory
    {
        int NumberOfMutantsExecutingInParallel { get; set; }

        CppBuildContext PrepareTestFiles(string testClass, string sourceClass, string sourceHeader, string testProject, string testSolution);

        void DeleteTestFiles(CppBuildContext context);
    }
}