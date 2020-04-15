using Microsoft.Extensions.CommandLineUtils;

namespace MuTest.CLI.Core
{
    public class CliOption<T>
    {
        public string ArgumentName { get; set; }

        public string ArgumentShortName { get; set; }

        public string ArgumentDescription { get; set; }

        public T DefaultValue { get; set; }

        public CommandOptionType ValueType { get; set; } = CommandOptionType.SingleValue;
    }
}
