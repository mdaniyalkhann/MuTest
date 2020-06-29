using System;
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

        public string SpecificLines { get; private set; }

        public CppMutantOrchestrator(IList<IMutator> mutators = null, string specificLines = "")
        {
            Mutators = mutators ?? new List<IMutator>
            {
                new AssignmentStatementMutator(),
                new ArithmeticOperatorMutator(),
                new BooleanMutator(),
                new RelationalOperatorMutator(),
                new LogicalConnectorMutator(),
                new PrePostfixUnaryMutator()
            };

            SpecificLines = specificLines;

            Mutants = new Collection<CppMutant>();
        }

        public static IEnumerable<CppMutant> GetDefaultMutants(string sourceFile, string specificLines = "")
        {
            if (sourceFile == null)
            {
                return new Collection<CppMutant>();
            }

            var orchestrator = new CppMutantOrchestrator(new List<IMutator>
            {
                new ArithmeticOperatorMutator(),
                new RelationalOperatorMutator(),
                new LogicalConnectorMutator(),
                new PrePostfixUnaryMutator()
            })
            {
                SpecificLines = specificLines
            };

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
                const char separator = ':';
                var lineNumber = 0;
                string line;
                var insideCommentedCode = false;
                var id = 0;

                int minimum = -1;
                int maximum = int.MaxValue;
                if (!string.IsNullOrWhiteSpace(SpecificLines))
                {
                    var range = SpecificLines.Split(separator);
                    minimum = Convert.ToInt32(range[0]);
                    maximum = Convert.ToInt32(range[1]);
                }

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    line = line.Trim();

                    if (lineNumber < minimum ||
                        lineNumber > maximum)
                    {
                        continue;
                    }

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
                        CommentLine commentLine = null;
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

                        for (var index = 0; index < line.Length; index++)
                        {
                            var character = line[index];

                            if (codeLine.StringLines.Any(x => index > x.Start && index < x.End))
                            {
                                continue;
                            }

                            if (character == '/' &&
                                index + 1 < line.Length &&
                                (line[index + 1] == '/' || line[index + 1] == '*') &&
                                commentLine == null)
                            {
                                commentLine = new CommentLine
                                {
                                    Start = index
                                };

                                if (line[index + 1] == '/')
                                {
                                    commentLine.End = line.Length;
                                    codeLine.CommentLines.Add(commentLine);
                                    break;
                                }

                                continue;
                            }

                            if (character == '*' &&
                                index + 1 < line.Length &&
                                line[index + 1] == '/')
                            {
                                if (commentLine != null)
                                {
                                    commentLine.End = index;
                                    codeLine.CommentLines.Add(commentLine);
                                }
                            }
                        }

                        foreach (var mutator in Mutators)
                        {
                            var cppMutants = mutator.Mutate(codeLine).ToList();
                            foreach (var mutant in cppMutants)
                            {
                                mutant.Id = id++;
                                Mutants.Add(mutant);
                            }
                        }
                    }
                }
            }
        }
    }
}