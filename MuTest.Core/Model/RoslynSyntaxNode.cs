using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.Model
{
    public class RoslynSyntaxNode : IAnalyzableNode
    {
        protected SyntaxNode SyntaxNode { get; }

        public RoslynSyntaxNode(SyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
            Parent = syntaxNode.Parent != null ? CreateRelativeNode(syntaxNode.Parent) : null;
            IsCompoundNode = CheckIsCompoundNode(syntaxNode);
            SyntaxType = GetSyntaxType(syntaxNode);
        }

        public bool IsCompoundNode { get; }
        public AnalyzableNodeSyntaxType SyntaxType { get; }
        public IAnalyzableNode Parent { get; }

        public IEnumerable<IAnalyzableNode> ChildNodes()
        {
            return SyntaxNode.ChildNodes().Select(CreateRelativeNode);
        }

        public IEnumerable<IAnalyzableNode> DescendantNodes()
        {
            return SyntaxNode.DescendantNodes().Select(CreateRelativeNode);
        }

        public IEnumerable<IAnalyzableNode> DescendantNodesAndSelf()
        {
            return SyntaxNode.DescendantNodesAndSelf().Select(CreateRelativeNode);
        }

        public bool IsInvocationOfMemberOfType(Type type)
        {
            return SyntaxNode is InvocationExpressionSyntax invocationExpressionSyntax
                   && invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                   && IsMemberAccessExpressionOfType(memberAccessExpressionSyntax, type);
        }

        public bool IsMemberAccessExpressionOfType(Type type)
        {
            if (!(SyntaxNode is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return false;
            }

            return IsMemberAccessExpressionOfType(memberAccessExpressionSyntax, type);
        }

        public bool IsInvocationOfMemberOfTypeBelongingToNamespace(string @namespace)
        {
            return SyntaxNode is InvocationExpressionSyntax invocationExpressionSyntax
                   && invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                   && IsMemberAccessExpressionOfTypeBelongingToNamespace(memberAccessExpressionSyntax, @namespace);
        }

        public bool IsMemberAccessExpressionOfTypeBelongingToNamespace(string @namespace)
        {
            if (!(SyntaxNode is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return false;
            }

            return IsMemberAccessExpressionOfTypeBelongingToNamespace(memberAccessExpressionSyntax, @namespace);
        }

        protected virtual bool IsMemberAccessExpressionOfTypeBelongingToNamespace(MemberAccessExpressionSyntax memberAccessExpressionSyntax, string @namespace)
        {
            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }
            // Not accurate enough, but it's the best we can do with the available data
            return memberAccessExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax
                   && identifierNameSyntax.Identifier.Text.Contains(@namespace);
        }




        protected virtual bool IsMemberAccessExpressionOfType(MemberAccessExpressionSyntax memberAccessExpressionSyntax, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return memberAccessExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax
                   && identifierNameSyntax.Identifier.Text == type.Name;
        }

        public string Kind()
        {
            return SyntaxNode.Kind().ToString();
        }

        public string GetText()
        {
            return SyntaxNode.GetText().ToString();
        }

        protected virtual IAnalyzableNode CreateRelativeNode(SyntaxNode syntaxNode)
        {
            return new RoslynSyntaxNode(syntaxNode);
        }

        private static AnalyzableNodeSyntaxType GetSyntaxType(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case BlockSyntax _:
                    return AnalyzableNodeSyntaxType.BlockSyntax;
                case BinaryExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.BinaryExpressionSyntax;
                case AssignmentExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.AssignmentExpressionSyntax;
                case LiteralExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.LiteralExpressionSyntax;
                case CheckedExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.CheckedExpressionSyntax;
                case InterpolatedStringExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.InterpolatedStringExpressionSyntax;
                case InvocationExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.InvocationExpressionSyntax;
                case ConditionalExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.ConditionalExpressionSyntax;
                case PostfixUnaryExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.PostfixUnaryExpressionSyntax;
                case PrefixUnaryExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.PrefixUnaryExpressionSyntax;
                case MemberAccessExpressionSyntax _:
                    return AnalyzableNodeSyntaxType.MemberAccessExpressionSyntax;
                case ArgumentListSyntax _:
                    return AnalyzableNodeSyntaxType.ArgumentListSyntax;
                case ArgumentSyntax _:
                    return AnalyzableNodeSyntaxType.ArgumentSyntax;
                case IdentifierNameSyntax _:
                    return AnalyzableNodeSyntaxType.IdentifierNameSyntax;
                case StatementSyntax _:
                    return AnalyzableNodeSyntaxType.StatementSyntax;
                case MethodDeclarationSyntax _:
                    return AnalyzableNodeSyntaxType.MethodDeclarationSyntax;
                case PropertyDeclarationSyntax _:
                    return AnalyzableNodeSyntaxType.PropertyDeclarationSyntax;
                default:
                    return AnalyzableNodeSyntaxType.Other;
            }
        }

        private static bool CheckIsCompoundNode(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case BlockSyntax _:
                case CheckedStatementSyntax _:
                case ForStatementSyntax _:
                case ForEachVariableStatementSyntax _:
                case CommonForEachStatementSyntax _:
                case DoStatementSyntax _:
                case EmptyStatementSyntax _:
                case IfStatementSyntax _:
                case LabeledStatementSyntax _:
                case LocalFunctionStatementSyntax _:
                case LockStatementSyntax _:
                case SwitchStatementSyntax _:
                case TryStatementSyntax _:
                case UnsafeStatementSyntax _:
                case UsingStatementSyntax _:
                case WhileStatementSyntax _:
                case ExpressionStatementSyntax _:
                    return true;
                default:
                    return false;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is RoslynSyntaxNode msBuildSyntaxNode && msBuildSyntaxNode.SyntaxNode.Equals(SyntaxNode);
        }

        public override int GetHashCode()
        {
            return SyntaxNode.GetHashCode();
        }
    }
}