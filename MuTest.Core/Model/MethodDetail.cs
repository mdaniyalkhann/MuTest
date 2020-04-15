using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Mutants;
using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class MethodDetail
    {
        [JsonProperty("name")]
        public string MethodName { get; set; }

        [JsonIgnore]
        public SyntaxNode Method { get; set; }

        [JsonIgnore]
        public List<CoverageDSPriv.LinesRow> Lines { get; } = new List<CoverageDSPriv.LinesRow>();

        [JsonProperty("coverage")]
        public Coverage Coverage { get; set; }

        [JsonIgnore]
        public bool CoverageExist => Coverage != null;

        [JsonIgnore]
        public string LinesCovered => CoverageExist
            ? $"{Coverage.LinesCovered}/{Coverage.TotalLines}"
            : string.Empty;

        [JsonIgnore]
        public string BlocksCovered => CoverageExist
            ? $"{Coverage.BlocksCovered}/{Coverage.TotalBlocks}"
            : string.Empty;

        [JsonIgnore]
        public string LineCoverage => CoverageExist
            ? $"[{Coverage.LinesCoveredPercentage}]"
            : "NA";

        [JsonIgnore]
        public string BranchCoverage => CoverageExist
            ? $"[{Coverage.BlocksCoveredPercentage}]"
            : "NA";

        [JsonIgnore]
        public List<MethodDetail> TestMethods { get; } = new List<MethodDetail>();

        [JsonIgnore]
        public List<string> ParentMethodNames { get; } = new List<string>();

        [JsonProperty("mutants")]
        public List<Mutant> Mutants { get; } = new List<Mutant>();

        [JsonIgnore]
        public IList<Mutant> SurvivedMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Survived).ToList();

        [JsonIgnore]
        public IList<Mutant> KilledMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Killed).ToList();

        [JsonIgnore]
        public IList<Mutant> NotCoveredMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.NotCovered).ToList();

        [JsonIgnore]
        public IList<Mutant> NotRunMutants => Mutants.Where(x => x.ResultStatus != MutantStatus.Killed &&
                                                                 x.ResultStatus != MutantStatus.Skipped &&
                                                                 x.ResultStatus != MutantStatus.NotCovered).ToList();

        [JsonIgnore]
        public IList<Mutant> CoveredMutants => Mutants.Where(x => x.ResultStatus != MutantStatus.NotCovered).ToList();

        [JsonIgnore]
        public IList<Mutant> TimeoutMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Timeout).ToList();

        [JsonIgnore]
        public IList<Mutant> BuildErrorMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.BuildError).ToList();

        [JsonIgnore]
        public IList<Mutant> SkippedMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Skipped).ToList();

        [JsonProperty("mutation-score")]
        public MutationScore MutationScore { get; } = new MutationScore();

        [JsonProperty("property")]
        public bool IsProperty { get; set; }

        [JsonProperty("constructor")]
        public bool IsConstructor { get; set; }

        [JsonProperty("override-method")]
        public bool IsOverrideMethod { get; set; }

        public void CalculateMutationScore()
        {
            MutationScore.Survived = SurvivedMutants.Count;
            MutationScore.Killed = KilledMutants.Count;
            MutationScore.Uncovered = NotCoveredMutants.Count;
            MutationScore.Timeout = TimeoutMutants.Count;
            MutationScore.BuildErrors = BuildErrorMutants.Count;
            MutationScore.Skipped = SkippedMutants.Count;
            MutationScore.Covered = CoveredMutants.Count - MutationScore.Timeout - MutationScore.BuildErrors - MutationScore.Skipped;
            MutationScore.Coverage = decimal.Divide(MutationScore.Killed, MutationScore.Covered == 0
                ? 1
                : MutationScore.Covered);
        }
    }
}