using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MuTest.Core.Common.StaticAnalyzers;

namespace Dashboard.Common
{
    public static class AssertsFileGenerator
    {
        public static string GenerateFile(this IList<AssertsAnalyzer.AssertMapper> asserts, string path, string className, string nameSpace)
        {
            var file = $"{path}{className}Test.Asserts.cs";
            var assertFile = File.CreateText(file);

            var assertBuilder = new StringBuilder();
            asserts.ToList().ForEach(x =>
            {
                assertBuilder.AppendLine($@"            try
            {{
                {x.Print()};
                Add({GetAssertion(x.Left, x.Right)});
            }}
            catch
            {{
                // ignored
            }}");
            });

            assertFile.Write(AssertTemplate, nameSpace, className, assertBuilder);
            CloseFile(assertFile);

            return file;
        }

        private static string GetAssertion(string left, string right)
        {
            switch (right)
            {
                case "true":
                    return $@"@""() => {left}.ShouldBeTrue(),""";
                case "false":
                    return $@"@""() => {left}.ShouldBeFalse(),""";
                case "null":
                    return $@"@""() => {left}.ShouldBeNull(),""";
                case "":
                case "\"\"":
                case "string.Empty":
                case " ":
                    return $@"@""() => {left}.ShouldBeNullOrWhiteSpace(),""";
                default:
                    return $@"@""() => {left}.ShouldBe({right.Replace("\"", "\"\"")}),""";
            }
        }

        private static void CloseFile(TextWriter writer)
        {
            writer?.Close();
            writer?.Dispose();
        }

        private static readonly string AssertTemplate = @"using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace {0}
{{
    public partial class {1}Test
    {{
        private readonly IList<string> _assertions = new List<string>();
        private IList<string> _assertionsCurrent;
        private void GenerateAsserts()
        {{
            _assertionsCurrent = new List<string>();
{2}
            Generate();
        }}

        private void Add(string expression)
        {{
            if(!_assertions.Contains(expression))
            {{
                _assertions.Add(expression);
                _assertionsCurrent.Add(expression);
            }}
        }}

        private void Generate()
        {{
            _assertionsCurrent.ToList().ForEach(x=> Console.WriteLine(x));
        }}
    }}
}}";
    }
}