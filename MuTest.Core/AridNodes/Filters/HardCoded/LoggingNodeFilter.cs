using System;
using MuTest.Core.Model;

namespace MuTest.Core.AridNodes.Filters.HardCoded
{
    public class LoggingNodeFilter : IAridNodeFilter
    {
        private const string Log4NetRootNamespace = "log4net";
        private const string NLogRootNamespace = "NLog";
        private const string SerilogRootNamespace = "Serilog";
        private const string LogGeneric = ".Log";

        public bool IsSatisfied(IAnalyzableNode node)
        {
            node = node ?? throw new ArgumentNullException(nameof(node));
            return node.IsInvocationOfMemberOfTypeNamespaceContainingText(Log4NetRootNamespace) ||
                   node.IsInvocationOfMemberOfTypeNamespaceContainingText(NLogRootNamespace) ||
                   node.IsInvocationOfMemberOfTypeNamespaceContainingText(LogGeneric) ||
                   node.IsInvocationOfMemberOfTypeNamespaceContainingText(SerilogRootNamespace);
        }
    }
}