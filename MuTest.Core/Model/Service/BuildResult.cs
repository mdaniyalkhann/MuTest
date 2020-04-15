using MuTest.Core.Common;

namespace MuTest.Core.Model.Service
{
    public class BuildResult
    {
        public string BuildOutput { get; set; }

        public Constants.BuildExecutionStatus Status { get; set; }
    }
}
