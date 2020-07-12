using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Model.ClassDeclarations
{
    public class ClassDeclarationWithSemantics : ClassDeclaration
    {
        public SemanticModel SemanticModel { get; }

        public ClassDeclarationWithSemantics(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) : base(classDeclarationSyntax)
        {
            SemanticModel = semanticModel;
        }
    }
}