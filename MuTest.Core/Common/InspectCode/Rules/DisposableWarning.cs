using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class DisposableWarning : IRule
    {
        private readonly SemanticModel _model;

        public DisposableWarning(SemanticModel model)
        {
            _model = model;
        }

        public string Description => "The variable/field implements IDisposable. Please call 'Dispose()' on it in case if you are not disposing it";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Missing+Dispose";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (_model == null)
            {
                return null;
            }

            if (!(node is ObjectCreationExpressionSyntax))
            {
                return null;
            }

            var symbolicInfo = _model.GetTypeInfo(node);
            if (symbolicInfo.Type == null)
            {
                return null;
            }

            var isIDisposable = symbolicInfo
                .Type
                .AllInterfaces.Any(x => x.Name == nameof(IDisposable) || x.MetadataName == nameof(IDisposable));

            return isIDisposable
                ? Inspection.Create(this, node.LineNumber() + 1, node.ToFullString())
                : null;
        }
    }
}
