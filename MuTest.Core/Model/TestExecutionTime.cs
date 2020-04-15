using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class TestExecutionTime
    {
        public TestExecutionTime(string testName, double executionTime)
        {
            TestName = testName;
            ExecutionTime = executionTime;
        }

        [JsonProperty("test-name")]
        public string TestName { get; }

        [JsonProperty("execution-time")]
        public double ExecutionTime { get;  }
    }
}
