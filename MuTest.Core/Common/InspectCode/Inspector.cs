using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MuTest.Core.Common.InspectCode.Rules;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode
{
    public static class Inspector
    {
        public static async Task<IList<Inspection>> FindInspections(SyntaxNode node, string filePath)
        {
            var inspections = new List<Inspection>();

            await Task.Run(() =>
            {
                IList<IRule> rules = new List<IRule>
                {
                    new ArrayToListInForEach(),
                    new UnnecessaryTestDataCase(),
                    new UnnecessaryTestCase(),
                    new SimplifyShims(),
                    new InappropriateUsageOfProperty(),
                    new ProtectedPublicConstants(),
                    new BlankCodeBlock(),
                    new ClassesAreNoun(),
                    new ContextualKeyword(),
                    new DefineStruct(),
                    new DisposableWarning(Compilation.SemanticModels[filePath]),
                    new SettablePublicCollection(Compilation.SemanticModels[filePath]),
                    new UnusedVariable(Compilation.UnusedVariables.FirstOrDefault(x=> x.FilePath.Equals(filePath))?.Locations),
                    new UnusedParameter(Compilation.UnusedVariables.FirstOrDefault(x=> x.FilePath.Equals(filePath))?.Locations),
                    new EnumWithoutDefaultValue(),
                    new ExceptionWithoutContext(),
                    new FieldAsProtectedOrPublic(),
                    new GeneralReservedException(),
                    new LiskovSubstitutionPrincipal(),
                    new MethodWithBoolArgument(),
                    new UnnecessaryUseOfShimsContext(),
                    new MethodWithGreaterThanSevenArguments(),
                    new SwitchWithoutDefaultCase(),
                    new AssertSingleItemWithUow(),
                    new DuplicateShimsDefinition(),
                    new TestEntireUow()
                };

                var descendentNodes = node.DescendantNodes().ToList();
                foreach (var descendentNode in descendentNodes)
                {
                    inspections
                        .AddRange(rules.Select(rule => rule.Analyze(descendentNode))
                            .Where(inspection => inspection != null));
                }

            });

            return inspections;
        }

        public static StringBuilder PrintInspections(IList<Inspection> inspections, string file)
        {
            var inspectionBuilder = new StringBuilder($"<fieldset style=\"border: 2px solid; padding: 15px;\">File: {file}".PrintWithPreTagImportant());
            foreach (var inspection in inspections)
            {
                var color = inspection.Rule.Severity == Constants.SeverityType.Yellow.ToString()
                    ? Constants.Colors.Gold
                    : inspection.Rule.Severity == Constants.SeverityType.Severe.ToString()
                        ? Constants.Colors.Orange
                        : Constants.Colors.Red;

                inspectionBuilder.Append("<fieldset style=\"border: 1px solid; padding: 10px;\">")
                    .Append($"Line Number: {inspection.LineNumber}".PrintImportantWithLegend(color: Constants.Colors.Blue))
                    .Append($"{"Description: ".PrintImportant()}{inspection.Rule.Description}".PrintWithPreTag())
                    .Append($"{"Severity: ".PrintImportant()}{inspection.Rule.Severity.PrintImportant(color: color)}".PrintWithPreTag())
                    .Append($"{"Code Review Url: ".PrintImportant()}<a href=\"{inspection.Rule.CodeReviewUrl}\">{inspection.Rule.CodeReviewUrl}</a>".PrintWithPreTag())
                    .Append("<fieldset style=\"border: 1px solid; padding: 5px;\">")
                    .Append("Code: ".PrintImportantWithLegend())
                    .Append(inspection.Code.Encode().PrintWithPreTag())
                    .Append("</fieldset>")
                    .Append("</fieldset>")
                    .Append("</fieldset>");
            }

            return inspectionBuilder;
        }
    }
}