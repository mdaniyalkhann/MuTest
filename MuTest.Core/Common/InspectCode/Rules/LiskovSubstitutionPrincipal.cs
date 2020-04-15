using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class LiskovSubstitutionPrincipal : IRule
    {
        private const string PublicModifier = "public";
        private const string ProtectedModifier = "protected";

        public string Description => "This member publicly exposes a concrete collection type";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/AQ_kEw";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            var predefinedType = node as PredefinedTypeSyntax;
            var parameterType = node as ParameterSyntax;
            var genericType = node as GenericNameSyntax;

            var collectionTypes = new[]
            {
                "Enumerable",
                "ReadOnlyCollection",
                "Collection",
                "ReadOnlyList",
                "Dictionary",
                "List"
            };

            if (predefinedType == null &&
                genericType == null &&
                parameterType == null)
            {
                return null;
            }

            if (genericType?.Parent is ObjectCreationExpressionSyntax ||
                genericType?.Parent is TypeOfExpressionSyntax ||
                genericType?.Parent is CastExpressionSyntax ||
                genericType?.Parent is BinaryExpressionSyntax ||
                genericType?.Parent is TypeArgumentListSyntax ||
                genericType?.Parent?.Parent is ArgumentSyntax ||
                predefinedType?.Parent is ObjectCreationExpressionSyntax ||
                predefinedType?.Parent is TypeOfExpressionSyntax ||
                predefinedType?.Parent is BinaryExpressionSyntax ||
                predefinedType?.Parent is TypeArgumentListSyntax ||
                predefinedType?.Parent is CastExpressionSyntax ||
                predefinedType?.Parent?.Parent is ArgumentSyntax ||
                parameterType?.Parent is TypeOfExpressionSyntax)
            {
                return null;
            }

            if (CheckPublicMethodOrField(genericType))
            {
                return null;
            }

            if (CheckPublicMethodOrField(predefinedType))
            {
                return null;
            }

            if (predefinedType?.IsVar == true ||
                genericType?.IsVar == true)
            {
                return null;
            }

            if (parameterType != null &&
                !(parameterType.Modifiers.ToString().Contains(PublicModifier) ||
                  parameterType.Modifiers.ToString().Contains(ProtectedModifier)))
            {
                return null;
            }

            var type = predefinedType?.ToString() ??
                       parameterType?.Type.ToString() ??
                       genericType.Identifier.ValueText ??
                       string.Empty;

            if (type.Contains("."))
            {
                type = type
                    .Split('.')
                    .Last();
            }

            if (!collectionTypes.Any(x => type.StartsWith(x)))
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }

        private static bool CheckPublicMethodOrField(SyntaxNode genericType)
        {
            var field = genericType?.Ancestors<FieldDeclarationSyntax>().FirstOrDefault();
            var method = genericType?.Ancestors<MethodDeclarationSyntax>().FirstOrDefault();
            var property = genericType?.Ancestors<PropertyDeclarationSyntax>().FirstOrDefault();

            if (field != null)
            {
                if (!(field.Modifiers.ToString().Contains(PublicModifier) ||
                      field.Modifiers.ToString().Contains(ProtectedModifier)))
                {
                    return true;
                }
            }

            if (method != null)
            {
                if (!(method.Modifiers.ToString().Contains(PublicModifier) ||
                      method.Modifiers.ToString().Contains(ProtectedModifier)))
                {
                    return true;
                }
            }

            if (property != null)
            {
                if (!(property.Modifiers.ToString().Contains(PublicModifier) ||
                      property.Modifiers.ToString().Contains(ProtectedModifier)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}