using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MuTest.Core.Mutators;
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
            var arrayTypes = new List<string>
            {
                "bool * ",
                "double * ",
                "float * ",
                "int * ",
                "long * ",
                "long double * ",
                "long int * ",
                "long long int * ",
                "short int * ",
                "signed char * ",
                "std::string * ",
                "unsigned char * ",
                "unsigned int * ",
                "unsigned long int * ",
                "unsigned long long int * ",
                "unsigned short int *",
                "wchar_t * ",
                "char * "
            };

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

                    if (line.CommentLines.Any(x => match.Index > x.Start && match.Index < x.End))
                    {
                        continue;
                    }

                    if (pattern == "!" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '=')
                    {
                        continue;
                    }

                    if (pattern == "-" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '>')
                    {
                        continue;
                    }

                    if (pattern == "-" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '-')
                    {
                        continue;
                    }

                    if (pattern == ">" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '=')
                    {
                        continue;
                    }

                    if (pattern == ">" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '>')
                    {
                        continue;
                    }

                    if (pattern == "<" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '<')
                    {
                        continue;
                    }

                    if (pattern == "<" &&
                        line.Line.Length > match.Index + 2 &&
                        line.Line[match.Index + 1] == '=')
                    {
                        continue;
                    }

                    if (pattern == ">" &&
                        match.Index > 0 &&
                        line.Line[match.Index - 1] == '-')
                    {
                        continue;
                    }

                    if (pattern == ">" &&
                        match.Index > 0 &&
                        line.Line[match.Index - 1] == '>')
                    {
                        continue;
                    }

                    if (pattern == "<" &&
                        match.Index > 0 &&
                        line.Line[match.Index - 1] == '<')
                    {
                        continue;
                    }

                    if (pattern == "-" &&
                        match.Index > 0 &&
                        line.Line[match.Index - 1] == '-')
                    {
                        continue;
                    }

                    if (pattern == " \\* " &&
                        arrayTypes.Any(x => line.Line.StartsWith(x) && match.Index <= x.Length))
                    {
                        continue;
                    }

                    foreach (var replacementValue in mutatorKinds[pattern])
                    {
                        var matchLength = match.Length;
                        var replacement = replacementValue;
                        var patternLength = match.Index + matchLength;
                        var mutation = new CppMutation
                        {
                            LineNumber = line.LineNumber,
                            OriginalNode = line.Line,
                            Type = MutatorType,
                            ReplacementNode = Regex.Replace(line.Line, pattern, node =>
                            {
                                if (node.Index == match.Index)
                                {
                                    if (replacement == "RL")
                                    {
                                        if (line.Line.Length > patternLength &&
                                            char.IsNumber(line.Line[patternLength]))
                                        {
                                            return " + 1 + ";
                                        }

                                        return line.StringLines.Any()
                                            ? " + \"mutest\" + "
                                            : " - ";
                                    }

                                    return replacement;
                                }

                                return match.Value;
                            })
                        };

                        mutation.DisplayName = $"Type: {MutatorType} - {mutation.OriginalNode} replace with {mutation.ReplacementNode}";

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
