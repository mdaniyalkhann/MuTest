using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public abstract class Mutator<T> where T : SyntaxNode
    {
        public abstract IEnumerable<Mutation> ApplyMutations(T node);

        public IEnumerable<Mutation> Mutate(SyntaxNode node)
        {
            if (node is T tNode)
            {
                return ApplyMutations(tNode);
            }

            return Enumerable.Empty<Mutation>();
        }
    }
}
