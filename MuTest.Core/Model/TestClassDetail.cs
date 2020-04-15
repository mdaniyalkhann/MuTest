using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Model
{
    public class TestClassDetail : ClassDetail
    {
        public bool PartialClassNodesAdded { get; set; }

        public IList<ClassDetail> PartialClasses { get; } = new List<ClassDetail>();

        public ClassDetail PartialClassWithSetupLogic { get; set; }

        public CompilationUnitSyntax BaseClass { get; set; }

        public bool SetupInBaseClass { get; set; }
    }
}
