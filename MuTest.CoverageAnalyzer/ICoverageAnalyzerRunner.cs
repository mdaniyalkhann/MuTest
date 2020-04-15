using MuTest.CoverageAnalyzer.Options;

namespace MuTest.CoverageAnalyzer
{
    public interface ICoverageAnalyzerRunner
    {
        void RunAnalyzer(CoverageAnalyzerOptions options);
    }
}