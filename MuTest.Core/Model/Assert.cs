namespace MuTest.Core.Model
{
    public class Assert
    {
        public Assert(string assertValue, string type)
        {
            Value = assertValue;
            Type = type;
        }

        public string Value { get; set; }

        public string Type { get; set; }

        public bool Skip { get; set; }
    }
}
