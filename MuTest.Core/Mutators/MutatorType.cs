using System.ComponentModel;

namespace MuTest.Core.Mutators
{
    public enum MutatorType
    {
        [Description("Arithmetic operators")]
        Arithmetic,
        [Description("Equality operators")]
        Equality,
        [Description("Boolean literals")]
        Boolean,
        [Description("Logical operators")]
        Logical,
        [Description("Assignment statements")]
        Assignment,
        [Description("Unary operators")]
        Unary,
        [Description("Update operators")]
        Update,
        [Description("Checked statements")]
        Checked,
        [Description("Linq methods")]
        Linq,
        [Description("Negate literals")]
        Negate,
        [Description("String literals")]
        String,
        [Description("Method calls")]
        MethodCall
    }
}
