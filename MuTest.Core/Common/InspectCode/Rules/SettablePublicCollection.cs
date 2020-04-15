using System.Collections;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class SettablePublicCollection : IRule
    {
        private const string PublicModifier = "public";
        private const string ProtectedModifier = "protected";
        private const string Getter = "get";
        private readonly SemanticModel _model;

        public SettablePublicCollection(SemanticModel model)
        {
            _model = model;
        }

        public string Description => "Public collections should not be externally settable";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Public+collections+should+not+be+externally+settable";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (_model == null)
            {
                return null;
            }

            if (!(node is PropertyDeclarationSyntax property))
            {
                return null;
            }

            var privateType = !(property.Modifiers.ToString().Contains(PublicModifier) ||
                                property.Modifiers.ToString().Contains(ProtectedModifier));
            if (privateType ||
                property.AccessorList == null ||
                property.AccessorList.Accessors.Count == 1 &&
                property.AccessorList.Accessors[0].ToFullString().Contains(Getter))
            {
                return null;
            }

            var symbolicInfo = _model.GetTypeInfo(node.ChildNodes().FirstOrDefault());
            if (symbolicInfo.Type == null)
            {
                return null;
            }

            var iEnumerable = symbolicInfo
                .Type
                .AllInterfaces.Any(x =>
                    x.Name == nameof(IEnumerable) ||
                    x.MetadataName == nameof(IEnumerable));

            return iEnumerable
                ? Inspection.Create(this, node.LineNumber() + 1, node.ToFullString())
                : null;
        }
    }
}