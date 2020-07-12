using System;
using System.Collections.Generic;

namespace MuTest.Core.Model.AridNodes
{
    public class NodesClassification
    {
        private readonly IDictionary<IAnalyzableNode, AridCheckResult> _results;

        internal NodesClassification(IDictionary<IAnalyzableNode, AridCheckResult> results)
        {
            _results = results;
        }

        public AridCheckResult GetResult(IAnalyzableNode syntaxNode)
        {
            if (!_results.ContainsKey(syntaxNode))
            {
                throw new InvalidOperationException($"No result exists for the {nameof(IAnalyzableNode)} provided.");
            }
            return _results[syntaxNode];
        }

        public bool TryGetResult(IAnalyzableNode syntaxNode, out AridCheckResult result)
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