using System.Text;

namespace MuTest.Core.Exceptions
{
    public class MuTestFailingTestException : MuTestInputException
    {
        public MuTestFailingTestException(string details = "") : base("\nTests are failing!", details)
        {
        }
    }
}