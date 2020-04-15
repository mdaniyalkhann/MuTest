using Microsoft.CodeAnalysis;

namespace MuTest.Core.Common.InspectCode
{
    public interface IRule
    {
        string Description { get; }

        string CodeReviewUrl { get; }

        string Severity { get; }

        Inspection Analyze(SyntaxNode node);
    }
}
