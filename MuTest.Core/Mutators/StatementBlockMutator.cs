using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class StatementBlockMutator : Mutator<BlockSyntax>, IMutator
    {
        public string Description { get; } = "BLOCK [{}]";

        public bool DefaultMutant { get; } = true;

        public override IEnumerable<Mutation> ApplyMutations(BlockSyntax node)
        {
            if (!node.DescendantNodes().Any())
            {
                yield break;
            }

            if (node.Parent is MethodDeclarationSyntax method &&
                !method.ReturnType.ToString().Equals("void", StringComparison.InvariantCultureIgnoreCase))
            {
                yield break;
            }

            var replacementNode = SyntaxFactory.Block();
            yield return new Mutation
            {
                OriginalNode = node,
                ReplacementNode = replacementNode,
                DisplayName = $"Block Statement mutation - remove node {string.Concat(node.ToString().Take(200))}...",
                Type = MutatorType.Block
            };
        }
    }
}