using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;
using MuTest.Core.Utility;

namespace MuTest.Core.Mutators
{
    public class StatementBlockMutator : Mutator<BlockSyntax>, IMutator
    {
        private const string BooleanInverter = "!";

        public string Description { get; } = "BLOCK [{}]";

        public bool DefaultMutant { get; } = true;

        public override IEnumerable<Mutation> ApplyMutations(BlockSyntax node)
        {
            if (!node.DescendantNodes().Any())
            {
                yield break;
            }

            SyntaxNode replacementNode = SyntaxFactory.Block();

            var method = node.Ancestors<MethodDeclarationSyntax>().FirstOrDefault();
            var property = node.Ancestors<PropertyDeclarationSyntax>().FirstOrDefault();

            var returnType = method?.ReturnType?.ToString() ?? property?.Type?.ToString();
            var randomValue = returnType?.GetRandomValue();

            if (!string.IsNullOrWhiteSpace(returnType) &&
                returnType != "void" &&
                (node.DescendantNodes<ReturnStatementSyntax>().Any() ||
                 node.DescendantNodes<YieldStatementSyntax>().Any()))
            {
                if (returnType == "bool" ||
                    returnType == "boolean")
                {
                    var returnNode = node.DescendantNodes<ReturnStatementSyntax>().FirstOrDefault();
                    if (returnNode?.Expression != null)
                    {
                        randomValue = returnNode.Expression.ToString().Trim();
                        randomValue = randomValue.StartsWith(BooleanInverter)
                            ? randomValue.TrimStart('!')
                            : $"{BooleanInverter}{randomValue}";
                    }
                    else
                    {
                        randomValue = "false";
                    }
                }

                replacementNode = SyntaxFactory.ParseStatement($"{{ return {randomValue}; }}");
            }

            yield return new Mutation
            {
                OriginalNode = node,
                ReplacementNode = replacementNode,
                DisplayName =
                    $"Block Statement mutation - node {string.Concat(node.ToString().Take(200))}... replace with {replacementNode}",
                Type = MutatorType.Block
            };
        }
    }
}