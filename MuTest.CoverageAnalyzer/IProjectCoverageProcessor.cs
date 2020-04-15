using System.Collections.Generic;

namespace MuTest.CoverageAnalyzer
{
    interface IClassExtractor
    {
        IEnumerable<Model.Class> ExtractClasses(Model.Project sourceProject);
    }
}
