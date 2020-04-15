using System.IO;
using System.Threading.Tasks;

namespace MuTest.Core.Common
{
    public interface ITestDirectoryFactory
    {
        int NumberOfMutantsExecutingInParallel { get; set; }

        string BuildExtensions { get; set; }

        void DeleteDirectories();

        FileInfo GetSourceCodeFile(int index);

        FileInfo GetProjectFile(int index);

        Task PrepareDirectoriesAndFiles();
    }
}