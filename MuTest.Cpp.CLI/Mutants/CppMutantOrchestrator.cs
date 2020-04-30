using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutators;

namespace MuTest.Cpp.CLI.Mutants
{
    public class CppMutantOrchestrator : IMutantOrchestrator
    {
        private ICollection<CppMutant> Mutants { get; set; }

        private IList<IMutator> Mutators { get; }

        public CppMutantOrchestrator(IList<IMutator> mutators = null)
        {
            Mutators = mutators ?? new List<IMutator>
            {
                new AssignmentStatementMutator(),
                new ArithmeticMutator(),
                new BooleanMutator(),
                new EqualityMutator(),
                new LogicalMutator(),
                new PrePostfixUnaryMutator()
            };

            Mutants = new Collection<CppMutant>();
        }

        public static IEnumerable<CppMutant> GetDefaultMutants(string sourceFile)
        {
            if (sourceFile == null)
            {
                new Collection<CppMutant>();
            }

            var orchestrator = new CppMutantOrchestrator(new List<IMutator>
            {
                new ArithmeticMutator(),
                new EqualityMutator(),
                new LogicalMutator()
            });

            orchestrator.Mutate(sourceFile);

            return orchestrator.GetLatestMutantBatch();
        }

        public IEnumerable<CppMutant> GetLatestMutantBatch()
        {
            var tempMutants = Mutants;
            Mutants = new Collection<CppMutant>();
            return tempMutants;
        }

        public void Mutate(string sourceFile)
        {
            Mutants = new Collection<CppMutant>();
            if (sourceFile == null)
            {
                return;
            }

            var skipList = new List<string>
            {
                "//",
                "#",
                "{",
                "}",
                "};",
                "});",
                ")",
                "assert(",
                "static_assert(",
                "public:",
                "private:",
                "protected:",
                "void",
                "using",
                "catch",
                "namespace",
                "typedef",
                "static",
                "static",
                "class"
            };

            using (var reader = new StreamReader(sourceFile))
            {
                var lineNumber = 0;
                string line;
                var insideCommentedCode = false;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    line = line.Trim();

                    if (string.IsNullOrWhiteSpace(line) ||
                        skipList.Any(x => line.StartsWith(x)) ||
                        line.EndsWith("):"))
                    {
                        continue;
                    }

                    if (line.StartsWith("/*"))
                    {
                        insideCommentedCode = true;
                    }

                    if (line.EndsWith("*/"))
                    {
                        insideCommentedCode = false;
                        continue;
                    }

                    if (!insideCommentedCode)
                    {
                        StringLine strLine = null;
                        var codeLine = new CodeLine
                        {
                            Line = line,
                            LineNumber = lineNumber
                        };

                        for (var index = 0; index < line.Length; index++)
                        {
                            var character = line[index];
                            if (character == '"' && strLine == null)
                            {
                                strLine = new StringLine
                                {
                                    Start = index
                                };

                                continue;
                            }

                            if (character == '"' &&
                                index != 0 &&
                                line[index - 1] != '\\' &&
                                index < line.Length - 1 &&
                                line[index + 1] != '"')
                            {
                                if (strLine != null)
                                {
                                    strLine.End = index;
                                    codeLine.StringLines.Add(strLine);
                                }
                            }
                        }

                        foreach (var mutator in Mutators)
                        {
                            var cppMutants = mutator.Mutate(codeLine).ToList();

                            foreach (var mutant in cppMutants)
                            {
                                Mutants.Add(mutant);
                            }
                        }
                    }
                }
            }
        }
    }
}