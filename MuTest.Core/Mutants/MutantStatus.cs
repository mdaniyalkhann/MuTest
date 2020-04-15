namespace MuTest.Core.Mutants
{
    public enum MutantStatus
    {
        NotRun = 0,
        Killed,
        Survived,
        Timeout,
        BuildError,
        NotCovered,
        Skipped
    }
}
