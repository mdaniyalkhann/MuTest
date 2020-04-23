using MuTest.Cpp.CLI.Model;

namespace MuTest.Cpp.CLI.Core
{
    public interface ICppDirectoryFactory
    {
        int NumberOfMutantsExecutingInParallel { get; set; }

        CppBuildContext PrepareTestFiles(CppClass cppClass);

        void DeleteTestFiles(CppBuildContext context);

        CppBuildContext PrepareSolutionFiles(CppClass cppClass);
    }
}