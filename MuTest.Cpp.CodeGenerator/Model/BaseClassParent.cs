#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class BaseClassParent
    {
        public long BaseCodeItemId { get; set; }
        public long ParentCodeItemId { get; set; }

        public virtual CodeItem BaseCodeItem { get; set; }
        public virtual CodeItem ParentCodeItem { get; set; }
    }
}
