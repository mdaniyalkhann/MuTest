using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Model;
using MuTest.Core.Model.CoverageAnalysis;

namespace MuTest.Core.Common.CoverageAnalyzers
{
    public class ClassCoverageAnalyzer : IClassCoverageAnalyzer
    {
        private readonly IList<CoverageDS> _codeCoverages;

        public ClassCoverageAnalyzer(IList<CoverageDS> codeCoverages)
        {
            _codeCoverages = codeCoverages;
        }

        public FindCoverageResult TryFindCoverage(string fullClassName, IEnumerable<string> classPaths, string assemblyName, DateTime? assemblyLastModificationDate, out Coverage coverage)
        {
            coverage = null;
            foreach (var codeCoverage in _codeCoverages)
            {
                if (codeCoverage != null)
                {
                    var coverages = codeCoverage
                        .Class
                        .Where(x =>
                        {
                            var coverageFullClassName = $"{x.NamespaceTableRow.NamespaceName}.{x.ClassName}";
                            return ClassNameMatches(fullClassName, coverageFullClassName) && AssemblyNameMatches(assemblyName, assemblyLastModificationDate, x);
                        }).ToList();
                    if (coverages.Count > 1)
                    {
                        coverage = null;
                        return FindCoverageResult.MultipleFound;
                    }
                    var coverageData = coverages.FirstOrDefault();
                    if (coverageData != null)
                    {
                        coverage = Coverage.Create(coverageData.LinesCovered, coverageData.LinesNotCovered, coverageData.BlocksCovered, coverageData.BlocksNotCovered);
                        break;
                    }
                    
                }
            }

            return coverage != null ? FindCoverageResult.Found : FindCoverageResult.NotFound;
        }

        private static bool ClassNameMatches(string fullClassName, string coverageFullClassName)
        {
            return string.Equals(coverageFullClassName, fullClassName, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool AssemblyNameMatches(string assemblyName, DateTime? assemblyCreationDate, CoverageDSPriv.ClassRow classRow)
        {
            var moduleName = classRow.NamespaceTableRow.ModuleName;
            if (string.Equals(moduleName, assemblyName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            // Next statements is to cover the case where 2 assemblies with the same name are on the same coverage report. In that case,
            // the report creator (vstestconsole or codecoverage) is attaching the creation date on the module name to differentiate the 2 assemblies.
            // e.g.
            // dataobjects.dll (1/17/2020 12:44:24 AM)	3800	10.43%	32334	88.79%
            // dataobjects.dll (1/17/2020 12:47:57 AM)	719	    79.62%	181	    20.04%

            if (assemblyCreationDate == null)
            {
                return false;
            }

            var assemblyIncludingCreationDateName = $"{assemblyName} ({assemblyCreationDate})";

            var matchesWithDifferentiatedAssemblyName = string.Equals(moduleName, assemblyIncludingCreationDateName, StringComparison.InvariantCultureIgnoreCase);
            return matchesWithDifferentiatedAssemblyName;
        }
    }
}