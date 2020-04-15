namespace MuTest.Core.Common.InspectCode
{
    public class Inspection
    {
        public IRule Rule { get; }

        private Inspection(IRule rule, int location, string code)
        {
            Rule = rule;
            LineNumber = location;
            Code = code;
        }

        public int LineNumber { get; }

        public string Code { get; }

        public static Inspection Create(IRule rule, int location, string code)
        {
            return new Inspection(rule, location, code);
        }
    }
}
