using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class UnusedVariable : IRule
    {
        private readonly IList<Location> _nodeLocations;

        public string Description => "Unused variable";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/gyCUEw";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public UnusedVariable(IList<Location> unusedVariables)
        {
            _nodeLocations = unusedVariables;
        }

        public Inspection Analyze(SyntaxNode node)
        {
            if (_nodeLocations == null || !_nodeLocations.Any())
            {
                return null;
            }

            if (!(node is VariableDeclaratorSyntax variable))
            {
                return null;
            }

            return _nodeLocations.All(x => x != variable.Identifier.GetLocation())
                ? null
                : Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
