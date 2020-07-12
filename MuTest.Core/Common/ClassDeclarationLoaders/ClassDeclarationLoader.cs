using MuTest.Core.Model.ClassDeclarations;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.ClassDeclarationLoaders
{
    public class ClassDeclarationLoader
    {
        public ClassDeclaration Load(string sourceFilePath, string className)
        {
            var classDeclarationSyntax = sourceFilePath.GetCodeFileContent().RootNode().ClassNode(className);
            return new ClassDeclaration(classDeclarationSyntax);
        }
    }
}