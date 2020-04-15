using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public interface IMutator
    {
        IEnumerable<Mutation> Mutate(SyntaxNode node);

        string Description { get; }

        bool DefaultMutant { get; }
    }
}
