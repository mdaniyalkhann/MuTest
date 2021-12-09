using System;
using System.Threading.Tasks;

namespace MuTest.Core.Common
{
    public interface ICoverageGenerator
    {
        Constants.ExecutionStatus CoverageStatus { get; }
        
        string HtmlCoveragePath { get; }
        
        event EventHandler<string> OutputDataReceived;
        
        Task Generate(string xmlCoverage, string sourceDir);
    }
}