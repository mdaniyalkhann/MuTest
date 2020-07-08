using System.Collections.Generic;
using System.Threading.Tasks;
using MuTest.Core.Mutators;

namespace MuTest.Core.Common
{
    public interface IMutantInitializer
    {
        string MutantFilterId { get; set; }

        string MutantFilterRegEx { get; set; }

        bool ExecuteAllTests { get; set; }

        string SpecificFilterRegEx { get; set; }

        int MutantsPerLine { get; set; }

        List<int> MutantsAtSpecificLines { get; }

        Task InitializeMutants(IList<IMutator> selectedMutators);
    }
}