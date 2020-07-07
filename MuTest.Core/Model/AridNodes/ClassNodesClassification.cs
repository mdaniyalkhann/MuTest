using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MuTest.Core.Model.AridNodes
{
    public class ClassNodesClassification
    {
        private readonly IDictionary<SyntaxNode, AridCheckResult> _results;

        internal ClassNodesClassification(IDictionary<SyntaxNode, AridCheckResult> results)
        {
            _results = results;
        }

        public AridCheckResult GetResult(SyntaxNode syntaxNode)
        {
            if (!_results.ContainsKey(syntaxNode))
            {
                throw new InvalidOperationException($"No result exists for the {nameof(SyntaxNode)} provided.");
            }
            return _results[syntaxNode];
        }

        public bool TryGetResult(SyntaxNode syntaxNode, out AridCheckResult result)
        {
            if (!_results.ContainsKey(syntaxNode))
            {
                result = null;
                return false;
            }

            result = _results[syntaxNode];
            return true;
        }
    }
}