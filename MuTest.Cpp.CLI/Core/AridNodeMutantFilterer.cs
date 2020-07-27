using System;
using System.Collections.Generic;
using System.Linq;
using MuTest.Cpp.CLI.Core.AridNodes;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Core
{
    public class AridNodeMutantFilterer : IAridNodeMutantFilterer
    {
        private readonly IAridNodeFilterProvider _aridNodeFilterProvider;

        public AridNodeMutantFilterer(IAridNodeFilterProvider aridNodeFilterProvider)
        {
            _aridNodeFilterProvider = aridNodeFilterProvider;
        }

        public IList<CppMutant> FilterMutants(IList<CppMutant> mutants)
        {
            mutants = mutants ?? throw new ArgumentNullException(nameof(mutants));
            return mutants.Where(mutant => !IsArid(mutant)).ToList();
        }

        private bool IsArid(CppMutant cppMutant)
        {
            cppMutant = cppMutant ?? throw new ArgumentNullException(nameof(cppMutant));
            return _aridNodeFilterProvider.Filters.Any(filter => filter.IsSatisfied(cppMutant.Mutation.OriginalNode));
        }
    }
}