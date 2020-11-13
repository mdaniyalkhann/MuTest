namespace MuTest.Cpp.CodeGenerator.Common
{
    public static class Extensions
    {
        public static string NormalizeName(this string name)
        {
            return name?
                .Replace("\u0001", string.Empty)
                .Replace("\u0002", string.Empty);
        }
    }
}
