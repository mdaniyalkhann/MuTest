using System.Collections.Generic;
using System.Linq;
using MuTest.Core.Mutators;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Core
{
    public class MutantSelector : IMutantSelector
    {
        private static readonly IList<MutatorType> MutatorsPriority = new List<MutatorType>
        {
            MutatorType.Relational,
            MutatorType.Logical,
            MutatorType.Block,
            MutatorType.Arithmetic,
            MutatorType.Unary
        };

        public IList<CppMutant> SelectMutants(int numberOfMutantsPerLine, IList<CppMutant> mutants)
        {
            if (numberOfMutantsPerLine < 1)
            {
                return mutants;
            }

            var selectedMutants = new List<CppMutant>();
            var mutantsByLine = mutants
                .GroupBy(grp => grp.Mutation.LineNumber)
                .Select(x => new
                {
                    Line = x.Key,
                    Mutants = x.OrderBy(pr => GetPriority(pr.Mutation.Type))
                }).ToList();


            foreach (var line in mutantsByLine)
            {
                selectedMutants.AddRange(line.Mutants.Take(numberOfMutantsPerLine));
            }

            return selectedMutants;
        }

        private static int GetPriority(MutatorType mutator)
        {
            return MutatorsPriority.Contains(mutator)
                ? MutatorsPriority.IndexOf(mutator)
                : MutatorsPriority.Count + 1;
        }
    }
}