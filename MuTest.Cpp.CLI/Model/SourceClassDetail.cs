using System;
using System.Collections.Generic;
using System.Linq;
using MuTest.Core.Model;
using MuTest.Core.Mutants;
using MuTest.Cpp.CLI.Mutants;
using Newtonsoft.Json;

namespace MuTest.Cpp.CLI.Model
{
    public class CppClass
    {
        [JsonProperty("mutation-score")]
        public MutationScore MutationScore { get; } = new MutationScore();

        [JsonProperty("execution-time")]
        public long ExecutionTime { get; set; }

        [JsonProperty("source-class")]
        public string SourceClass { get; set; }

        [JsonProperty("source-header")]
        public string SourceHeader { get; set; }

        [JsonProperty("test-class")]
        public string TestClass { get; set; }

        [JsonProperty("test-project")]
        public string TestProject { get; set; }

        [JsonProperty("mutants")]
        public List<CppMutant> Mutants { get; } = new List<CppMutant>();

        [JsonIgnore]
        public IList<CppMutant> SurvivedMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Survived).ToList();

        [JsonIgnore]
        public IList<CppMutant> KilledMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Killed).ToList();

        [JsonIgnore]
        public IList<CppMutant> NotCoveredMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.NotCovered).ToList();

        [JsonIgnore]
        public IList<CppMutant> NotRunMutants => Mutants.Where(x => x.ResultStatus != MutantStatus.Killed &&
                                                                 x.ResultStatus != MutantStatus.Skipped &&
                                                                 x.ResultStatus != MutantStatus.NotCovered).ToList();

        [JsonIgnore]
        public IList<CppMutant> CoveredMutants => Mutants.Where(x => x.ResultStatus != MutantStatus.NotCovered).ToList();

        [JsonIgnore]
        public IList<CppMutant> TimeoutMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Timeout).ToList();

        [JsonIgnore]
        public IList<CppMutant> BuildErrorMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.BuildError).ToList();

        [JsonIgnore]
        public IList<CppMutant> SkippedMutants => Mutants.Where(x => x.ResultStatus == MutantStatus.Skipped).ToList();

        [JsonProperty("configuration")]
        public string Configuration { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("test-solution")]
        public string TestSolution { get; set; }

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

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(TestClass))
            {
                throw new ArgumentNullException(nameof(TestClass));
            }

            if (string.IsNullOrWhiteSpace(SourceClass))
            {
                throw new ArgumentNullException(nameof(SourceClass));
            }

            if (string.IsNullOrWhiteSpace(SourceHeader))
            {
                throw new ArgumentNullException(nameof(SourceHeader));
            }

            if (string.IsNullOrWhiteSpace(TestProject))
            {
                throw new ArgumentNullException(nameof(TestProject));
            }

            if (string.IsNullOrWhiteSpace(TestSolution))
            {
                throw new ArgumentNullException(nameof(TestSolution));
            }
        }
    }
}