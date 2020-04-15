using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Model
{
    public class SyntaxFile
    {
        public CompilationUnitSyntax CompilationUnitSyntax { get; set; }

        public string FileName { get; set; }
    }
}
