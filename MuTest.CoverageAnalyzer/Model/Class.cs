using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.CoverageAnalyzer.Model
{
    public class Class
    {
        public ClassDeclarationSyntax DeclarationSyntax { get; set; }

        public string FullClassName { get; set; }

        public string FilePath { get; internal set; }
    }
}
