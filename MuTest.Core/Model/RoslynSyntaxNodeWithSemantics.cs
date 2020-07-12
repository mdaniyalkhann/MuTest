using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Model
{
    public class RoslynSyntaxNodeWithSemantics : RoslynSyntaxNode
    {
        private readonly SemanticModel _semanticModel;
        public RoslynSyntaxNodeWithSemantics(SyntaxNode syntaxNode, SemanticModel semanticModel) : base(syntaxNode)
        {
            _semanticModel = semanticModel;
        }

        protected override bool IsMemberAccessExpressionOfType(MemberAccessExpressionSyntax memberAccessExpressionSyntax, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return _semanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression).Type?.ToDisplayString() == type.FullName;
        }

        protected override IAnalyzableNode CreateRelativeNode(SyntaxNode syntaxNode)
        {
            return new RoslynSyntaxNodeWithSemantics(syntaxNode, _semanticModel);
        }
    }
}