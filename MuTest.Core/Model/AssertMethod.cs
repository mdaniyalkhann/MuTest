using System.Collections.Generic;

namespace MuTest.Core.Model
{
    public class AssertMethod
    {
        public string Method { get; set; }

        public IList<Assert> Asserts { get; set; } = new List<Assert>();
    }
}
