using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Model.ClassDeclarations
{
    public class ClassDeclaration
    {
        public ClassDeclaration(ClassDeclarationSyntax classDeclarationSyntax)
        {
            Syntax = classDeclarationSyntax;
        }

        public ClassDeclarationSyntax Syntax { get; set; }
    }
}