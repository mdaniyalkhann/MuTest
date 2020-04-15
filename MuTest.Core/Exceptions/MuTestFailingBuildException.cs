namespace MuTest.Core.Exceptions
{
    public class MuTestFailingBuildException : MuTestInputException
    {
        public MuTestFailingBuildException(string details = "") : base("\nTest Project Build is failed!", details)
        {
        }
    }
}