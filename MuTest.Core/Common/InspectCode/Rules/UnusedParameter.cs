using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class UnusedParameter : IRule
    {
        private readonly IList<Location> _nodeLocations;

        public string Description => "Unused parameter";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/gyCUEw";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public UnusedParameter(IList<Location> unusedParameters)
        {
            _nodeLocations = unusedParameters;
        }

        public Inspection Analyze(SyntaxNode node)
        {
            if (_nodeLocations == null || !_nodeLocations.Any())
            {
                return null;
            }

            if (!(node is ParameterSyntax parameter))
            {
                return null;
            }

            return _nodeLocations.All(x => x != parameter.Identifier.GetLocation())
                ? null
                : Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
