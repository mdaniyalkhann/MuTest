using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MuTest.Core.Mutators;
using MuTest.Core.Utility;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Mutators
{
    internal abstract class Mutator
    {
        protected IDictionary<string, IList<string>> KindsToMutate { get; set; } = new Dictionary<string, IList<string>>();

        public abstract MutatorType MutatorType { get; }

        public virtual IList<CppMutant> ApplyMutations(CodeLine line)
        {
            var mutants = new List<CppMutant>();

            var mutatorKinds = KindsToMutate;
            foreach (var pattern in mutatorKinds.Keys)
            {
                var matches = Regex.Matches(line.Line, pattern);
                foreach (Match match in matches)
                {
                    if (line.StringLines.Any(x => match.Index > x.Start && match.Index < x.End))
                    {
                        continue;
                    }
                    foreach (var replacement in mutatorKinds[pattern])
                    {
                        var mutation = new CppMutation
                        {
                            LineNumber = line.LineNumber,
                            OriginalNode = line.Line,
                            Type = MutatorType,
                            ReplacementNode = Regex.Replace(line.Line, pattern, node =>
                            {
                                if (node.Index == match.Index)
                                {
                                    return replacement;
                                }

                                return match.Value;
                            })
                        };

                        mutation.DisplayName = $"Line Number: {line.LineNumber} - Type: {MutatorType} - {mutation.OriginalNode} replace with {mutation.ReplacementNode}";

                        mutants.Add(new CppMutant
                        {
                            Mutation = mutation
                        });
                    }
                }
            }

            return mutants;
        }

        public IEnumerable<CppMutant> Mutate(CodeLine line)
        {
            return ApplyMutations(line);
        }
    }
}
