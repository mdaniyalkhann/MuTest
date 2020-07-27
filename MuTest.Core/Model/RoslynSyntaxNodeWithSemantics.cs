using System;
using System.Collections.Generic;
using System.Linq;
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

            var memberTypeFullName = _semanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression).Type
                ?.ToDisplayString();
            return memberTypeFullName == type.FullName;
        }

        protected override bool IsMemberAccessExpressionOfTypeBelongingToNamespace(
            MemberAccessExpressionSyntax memberAccessExpressionSyntax,
            string @namespace)
        {
            var memberNamespaceFullName = GetMemberNamespaceFullName(memberAccessExpressionSyntax);
            return memberNamespaceFullName == @namespace;
        }

        private string GetMemberNamespaceFullName(MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            memberAccessExpressionSyntax = memberAccessExpressionSyntax ??
                                           throw new ArgumentNullException(nameof(memberAccessExpressionSyntax));
            return _semanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression).Type
                ?.ContainingNamespace?.ToDisplayString();
        }

        protected override IAnalyzableNode CreateRelativeNode(SyntaxNode syntaxNode)
        {
            return new RoslynSyntaxNodeWithSemantics(syntaxNode, _semanticModel);
        }

        protected override bool IsMemberAccessExpressionOfTypeNamespaceContainingText(
            MemberAccessExpressionSyntax memberAccessExpressionSyntax,
            string text)
        {
            var namespaceName = GetMemberNamespaceFullName(memberAccessExpressionSyntax);
            return namespaceName.Contains(text);
        }
    }
}