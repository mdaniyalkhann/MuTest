using System.Collections.Generic;

namespace MuTest.CoverageAnalyzer.Model
{
    public class Solution
    {
        public string FullName { get; set; }

        public List<Project> SourceProjects { get; } = new List<Project>();

        public List<Project> TestProjects { get; } = new List<Project>();

        public Project CommonFakesProject { get; set; }
    }
}
