using System.Collections.Generic;
using MuTest.Core.Model;
using MuTest.Core.Model.CoverageAnalysis;
using System;

namespace MuTest.Core.Common.CoverageAnalyzers
{
    public interface IClassCoverageAnalyzer
    {
        FindCoverageResult TryFindCoverage(string fullClassName, IEnumerable<string> classPaths, string assemblyName, DateTime? ssemblyLastModificationDate, out Coverage coverage);
    }
}