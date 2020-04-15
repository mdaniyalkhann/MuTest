using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Model;

namespace MuTest.Core.Common
{
    public interface ICoverageAnalyzer
    {
        string Output { get; set; }
        void FindCoverage(SourceClassDetail source, CoverageDS codeCoverage);
    }
}