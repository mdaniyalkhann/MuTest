using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutators;
using MuTest.Cpp.CLI.Utility;

namespace MuTest.Cpp.CLI.Mutants
{
    public class CppMutantOrchestrator : IMutantOrchestrator
    {
        private const string BlockOpen = "{";
        private const string BlockClosed = "}";
        private const string If = "if";
        private const string Delete = "delete";
        private const string Trace = "trace";
        private const string Class = "class";
        private const string Static = "static";
        private const string Typedef = "typedef";
        private const string Namespace = "namespace";
        private const string Catch = "catch";
        private const string Using = "using";
        private const string VoId = "void";
        private const string ElseIf = "else if";
        private const string Extern = "extern";

        public static IList<IMutator> AllMutators => new List<IMutator>
        {
            new AssignmentStatementMutator(),
            new ArithmeticOperatorMutator(),
            new BooleanMutator(),
            new RelationalOperatorMutator(),
            new LogicalConnectorMutator(),
            new PrePostfixUnaryMutator()
        };

        public static IList<IMutator> DefaultMutators =>
            new List<IMutator>
            {
                new ArithmeticOperatorMutator(),
                new RelationalOperatorMutator(),
                new LogicalConnectorMutator(),
                new PrePostfixUnaryMutator()
            };

        public string SpecificLines { get; private set; }

        private ICollection<CppMutant> Mutants { get; set; }

        private IList<IMutator> Mutators { get; }

        public CppMutantOrchestrator(IList<IMutator> mutators = null, string specificLines = "")
        {
            Mutators = mutators ?? AllMutators;

            SpecificLines = specificLines;

            Mutants = new Collection<CppMutant>();
        }

        public static IEnumerable<CppMutant> GetDefaultMutants(string sourceFile, string specificLines = "")
        {
            if (sourceFile == null)
            {
                return new Collection<CppMutant>();
            }

            var orchestrator = new CppMutantOrchestrator(DefaultMutators)
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
            if (sourceFile == null || !File.Exists(sourceFile))
            {
                return;
            }

            var codeLines = File.ReadAllLines(sourceFile);

            var skipList = new List<string>
            {
                "//",
                "#",
                BlockOpen,
                BlockClosed,
                "};",
                "});",
                ")",
                "public:",
                "private:",
                "protected:",
                Extern,
                VoId,
                Using,
                Catch,
                Namespace,
                Typedef,
                Static,
                Class,
                Trace,
                Delete
            };

            const char separator = ':';
            var lineNumber = 0;
            string line;
            var insideCommentedCode = false;
            var id = 0;

            var minimum = -1;
            var maximum = int.MaxValue;
            if (!string.IsNullOrWhiteSpace(SpecificLines))
            {
                var range = SpecificLines.Split(separator);
                minimum = Convert.ToInt32(range[0]);
                maximum = Convert.ToInt32(range[1]);
            }

            for (var lineIndex = 0; lineIndex < codeLines.Length; lineIndex++)
            {
                lineNumber++;
                line = codeLines[lineIndex].Trim();

                if (lineNumber < minimum ||
                    lineNumber > maximum)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) ||
                    skipList.Any(x => line.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)) ||
                    line.EndsWith("):"))
                {
                    continue;
                }

                if (line.StartsWith("/*") || line.EndsWith("/*"))
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
                    var codeLine = new CodeLine
                    {
                        Line = line,
                        LineNumber = lineNumber,
                        EndLineNumber = lineNumber
                    };

                    AddStringsInsideLines(line, codeLine);
                    AddCommentsInsideLine(line, codeLine);
                    AddIfBlocks(line, codeLine, lineIndex, codeLines);

                    foreach (var mutator in Mutators)
                    {
                        var cppMutants = mutator.Mutate(codeLine).ToList();
                        foreach (var mutant in cppMutants)
                        {
                            mutant.Id = id++;
                            mutant.Mutation.EndLineNumber = codeLine.EndLineNumber;
                            Mutants.Add(mutant);
                        }
                    }
                }
            }
        }

        private static void AddIfBlocks(string code, CodeLine codeLine, int lineIndex, IReadOnlyList<string> codeLines)
        {
            var countOpenBrackets = 0;
            var countCloseBrackets = 0;
            var line = code.Trim().RemoveComments();
            if (line.StartsWith(If) || line.StartsWith(ElseIf))
            {
                if (line.EndsWith(BlockOpen))
                {
                    countOpenBrackets++;
                }

                if (line.EndsWith(BlockClosed))
                {
                    codeLine.EndLineNumber = lineIndex + 1;
                }
                else
                {
                    var insideCommentedCode = false;
                    for (var nestedIndex = lineIndex + 1; nestedIndex < codeLines.Count; nestedIndex++)
                    {
                        var nestedCode = codeLines[nestedIndex].Trim();

                        if (nestedCode.StartsWith("/*") || line.EndsWith("/*"))
                        {
                            insideCommentedCode = true;
                        }

                        if (nestedCode.EndsWith("*/"))
                        {
                            insideCommentedCode = false;
                            continue;
                        }

                        if (!insideCommentedCode)
                        {
                            if (nestedCode.StartsWith(BlockOpen) ||
                                nestedCode.EndsWith(BlockOpen) ||
                                nestedCode.Trim('}').Trim().EndsWith(BlockOpen))
                            {
                                countOpenBrackets++;
                            }

                            if (nestedCode.StartsWith(BlockClosed) ||
                                nestedCode.EndsWith(BlockClosed) ||
                                nestedCode.Trim('{').Trim().EndsWith(BlockClosed))
                            {
                                countCloseBrackets++;
                            }

                            if (countOpenBrackets > 0 && countCloseBrackets == countOpenBrackets)
                            {
                                codeLine.EndLineNumber = nestedIndex + 1;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void AddCommentsInsideLine(string line, CodeLine codeLine)
        {
            CommentLine commentLine = null;
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
        }

        private static void AddStringsInsideLines(string line, CodeLine codeLine)
        {
            StringLine strLine = null;
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
        }
    }
}