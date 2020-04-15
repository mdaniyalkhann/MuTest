using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Model.Service
{
    public class TestResult
    {
        public string TestOutput { get; set; }

        public TestExecutionStatus Status { get; set; } = TestExecutionStatus.Success;
        public string CoveragePath { get; set; }
    }
}
