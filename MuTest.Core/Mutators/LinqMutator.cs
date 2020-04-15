using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class LinqMutator : Mutator<ExpressionSyntax>, IMutator
    {
        public string Description { get; } = "LINQ [First, Last, All, Any, Min, Max...]";

        public bool DefaultMutant { get; } = false;

        private IReadOnlyDictionary<LinqExpression, LinqExpression> KindsToMutate { get; }

        public LinqMutator()
        {
            KindsToMutate = new Dictionary<LinqExpression, LinqExpression>
            {
                [LinqExpression.FirstOrDefault] = LinqExpression.First,
                [LinqExpression.First] = LinqExpression.FirstOrDefault,
                [LinqExpression.SingleOrDefault] = LinqExpression.Single,
                [LinqExpression.Single] = LinqExpression.SingleOrDefault,
                [LinqExpression.Last] = LinqExpression.First,
                [LinqExpression.All] = LinqExpression.Any,
                [LinqExpression.Any] = LinqExpression.All,
                [LinqExpression.Skip] = LinqExpression.Take,
                [LinqExpression.Take] = LinqExpression.Skip,
                [LinqExpression.SkipWhile] = LinqExpression.TakeWhile,
                [LinqExpression.TakeWhile] = LinqExpression.SkipWhile,
                [LinqExpression.Min] = LinqExpression.Max,
                [LinqExpression.Max] = LinqExpression.Min,
                [LinqExpression.Sum] = LinqExpression.Count,
                [LinqExpression.Count] = LinqExpression.Sum,
                [LinqExpression.OrderBy] = LinqExpression.OrderByDescending,
                [LinqExpression.OrderByDescending] = LinqExpression.OrderBy,
                [LinqExpression.ThenBy] = LinqExpression.ThenByDescending,
                [LinqExpression.ThenByDescending] = LinqExpression.ThenBy
            };
            RequireArguments = new HashSet<LinqExpression>
            {
                LinqExpression.All,
                LinqExpression.SkipWhile,
                LinqExpression.TakeWhile,
                LinqExpression.Sum,
                LinqExpression.OrderBy,
                LinqExpression.OrderByDescending,
                LinqExpression.ThenBy,
                LinqExpression.ThenByDescending
            };
        }

        public HashSet<LinqExpression> RequireArguments { get; }

        public override IEnumerable<Mutation> ApplyMutations(ExpressionSyntax node)
        {
            var original = node;
            if (node.Parent is ConditionalAccessExpressionSyntax || node.Parent is MemberAccessExpressionSyntax)
            {
                yield break;
            }

            foreach (var mutation in FindMutableMethodCalls(node, original))
            {
                yield return mutation;
            }
        }

        private IEnumerable<Mutation> FindMutableMethodCalls(ExpressionSyntax node, ExpressionSyntax original)
        {
            while(node is ConditionalAccessExpressionSyntax conditional)
            {
                foreach (var subMutants in FindMutableMethodCalls(conditional.Expression, original))
                {
                    yield return subMutants;
                }
                node = conditional.WhenNotNull;
            }

            for (;;)
            {
                ExpressionSyntax next = null;
                if (!(node is InvocationExpressionSyntax invocationExpression))
                {
                    yield break;
                }

                string memberName;
                SyntaxNode toReplace;
                switch (invocationExpression.Expression)
                {
                    case MemberAccessExpressionSyntax memberAccessExpression:
                        toReplace = memberAccessExpression.Name;
                        memberName = memberAccessExpression.Name.Identifier.ValueText;
                        next = memberAccessExpression.Expression;
                        break;
                    case MemberBindingExpressionSyntax memberBindingExpression:
                        toReplace = memberBindingExpression.Name;
                        memberName = memberBindingExpression.Name.Identifier.ValueText;
                        break;
                    default:
                        yield break;
                }

                if (Enum.TryParse(memberName, out LinqExpression expression) &&
                    KindsToMutate.TryGetValue(expression, out var replacementExpression))
                {
                    if (RequireArguments.Contains(replacementExpression) &&
                        invocationExpression.ArgumentList.Arguments.Count == 0)
                    {
                        yield break;
                    }

                    var replacement = original.ReplaceNode(toReplace,
                        SyntaxFactory.IdentifierName(replacementExpression.ToString()));

                    yield return new Mutation
                    {
                        DisplayName =
                            $"Linq method mutation ({original}() replace with {replacement}())",
                        OriginalNode = original,
                        ReplacementNode = replacement,
                        Type = MutatorType.Linq
                    };
                }

                node = next;
            }
        }
    }

    /// <summary> Enumeration for the different kinds of linq expressions </summary>
    public enum LinqExpression
    {
        OrderBy,
        OrderByDescending,
        FirstOrDefault,
        First,
        SingleOrDefault,
        Single,
        Last,
        All,
        Any,
        Skip,
        Take,
        SkipWhile,
        TakeWhile,
        Min,
        Max,
        Sum,
        Count,
        ThenBy,
        ThenByDescending
    }
}