using System;
using System.Collections.Generic;
using System.Linq;
using MuTest.Core.Model;
using MuTest.Core.Model.CoverageAnalysis;
using MuTest.Core.Model.CoverageReports;

namespace MuTest.Core.Common.CoverageAnalyzers
{
    public class CoberturaClassCoverageAnalyzer : IClassCoverageAnalyzer
    {
        private readonly CoberturaCoverageReport _report;

        public CoberturaClassCoverageAnalyzer(CoberturaCoverageReport report)
        {
            _report = report;
        }

        public FindCoverageResult TryFindCoverage(string fullClassName, IEnumerable<string> classPaths, string assemblyName, DateTime? assemblyLastModificationDate, out Coverage coverage)
        {
            var coverages = new List<Coverage>();
            foreach (var classPath in classPaths)
            {
                if (TryFindCoverage(fullClassName, classPath, out coverage))
                {
                    coverages.Add(coverage);
                }
            }

            coverage = coverages.Any() ? SumCoverages(coverages) : null;
            return coverages.Any() ? FindCoverageResult.Found : FindCoverageResult.NotFound;
        }

        private static Coverage SumCoverages(IReadOnlyCollection<Coverage> coverages)
        {
            return Coverage.Create(
                (uint) coverages.Sum(c => c.LinesCovered),
                (uint) coverages.Sum(c => c.LinesNotCovered),
                (uint) coverages.Sum(c => c.BlocksCovered), 
                (uint) coverages.Sum(c => c.BlocksNotCovered));
        }

        private bool TryFindCoverage(string fullClassName, string classPath, out Coverage coverage)
        {
            var fileElement = FindChildHavingPath(classPath, _report.results.children);
            if (fileElement == null)
            {
                coverage = null;
                return false;
            }

            var plainClassName = fullClassName.Split('.').Last();
            var classElement = fileElement.children.FirstOrDefault(c => c.name == fullClassName) ??
                               fileElement.children.FirstOrDefault(c => c.name == plainClassName);
            if (classElement == null)
            {
                coverage = null;
                return false;
            }

            var lineCoverage = classElement.elements.FirstOrDefault(e => e.name == "Lines");
            var branchCoverage = classElement.elements.FirstOrDefault(e => e.name == "Conditionals");
            var linesCovered = (uint) lineCoverage.numerator;
            var allLines = Convert.ToUInt32(lineCoverage.denominator);
            var linesNotCovered = allLines - linesCovered;
            var branchesCovered = Convert.ToUInt32(branchCoverage.numerator);
            var allBranches = Convert.ToUInt32(branchCoverage.denominator);
            var branchesNotCovered = allBranches - branchesCovered;
            coverage = Coverage.Create(linesCovered, linesNotCovered, branchesCovered, branchesNotCovered);

            return true;
        }

        private static Child FindChildHavingPath(string path, IList<Child> children)
        {
            var child = children.FirstOrDefault(c => PathsMatch(path, c.name));
            if (child != null)
            {
                return child;
            }

            return children.Select(grandChild => FindChildHavingPath(path, grandChild.children))
                .FirstOrDefault(matchingChild => matchingChild != null);
        }

        private static bool PathsMatch(string path1, string path2)
        {
            var path1Normalized = path1.Replace("\\", "/");
            var path2Normalized = path2.Replace("\\", "/");
            return string.Equals(path1Normalized, path2Normalized, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}