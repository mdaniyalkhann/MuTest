namespace MuTest.CoverageAnalyzer.Model
{
    public class Project
    {
        public string AbsolutePath { get; set; }

        public string ProjectName { get; set; }

        public string RelativePath { get; set; }

        public Solution Solution { get; set; }
    }
}
