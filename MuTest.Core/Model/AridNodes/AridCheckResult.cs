using System;
using MuTest.Core.AridNodes.Filters;

namespace MuTest.Core.Model.AridNodes
{
    public class AridCheckResult
    {
        public static AridCheckResult CreateForArid(params IAridNodeFilter[] triggeredBy)
        {
            return new AridCheckResult(true, triggeredBy);
        }

        public static AridCheckResult CreateForNonArid()
        {
            return new AridCheckResult(false, Array.Empty<IAridNodeFilter>());
        }

        private AridCheckResult(bool isArid, IAridNodeFilter[] triggeredBy)
        {
            IsArid = isArid;
            TriggeredBy = triggeredBy;
        }

        public bool IsArid { get; }
        public IAridNodeFilter[] TriggeredBy { get; }
    }
}