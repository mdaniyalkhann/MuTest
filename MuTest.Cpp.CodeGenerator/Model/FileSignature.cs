#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class FileSignature
    {
        public long FileId { get; set; }
        public long Kind { get; set; }
        public byte[] Signature { get; set; }

        public virtual File File { get; set; }
    }
}
