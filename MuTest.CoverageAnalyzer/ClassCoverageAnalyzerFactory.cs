using System;
using MuTest.Core.Common.CoverageAnalyzers;
using MuTest.CoverageAnalyzer.Options;

namespace MuTest.CoverageAnalyzer
{
    public class ClassCoverageAnalyzerFactory
    {
        public IClassCoverageAnalyzer Create(CoverageAnalyzerOptions options)
        {
            if (string.Equals(options.CodeCoverageType, CodeCoverageTypes.Cobertura,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return new CoberturaClassCoverageAnalyzer(options.CoberturaCoverageReport);
            }
            return new ClassCoverageAnalyzer(options.CodeCoverages);
        }
    }
}