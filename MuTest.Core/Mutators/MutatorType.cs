using System.ComponentModel;

namespace MuTest.Core.Mutators
{
    public enum MutatorType
    {
        [Description("Arithmetic operators")]
        Arithmetic,
        [Description("Relational operators")]
        Relational,
        [Description("Boolean literals")]
        Boolean,
        [Description("Logical operators")]
        Logical,
        [Description("Assignment statements")]
        Assignment,
        [Description("Unary operators")]
        Unary,
        [Description("Checked statements")]
        Checked,
        [Description("Linq methods")]
        Linq,
        [Description("Negate literals")]
        Negate,
        [Description("String literals")]
        String,
        [Description("Method calls")]
        MethodCall,
        [Description("Bitwise operators")]
        Bitwise,
        [Description("Block Statement")]
        Block
    }
}
