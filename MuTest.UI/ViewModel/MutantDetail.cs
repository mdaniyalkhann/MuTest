using MuTest.Core.Mutants;

namespace Dashboard.ViewModel
{
    public class MutantDetail
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual int ParentId { get; set; }

        public virtual MutantLevel Level { get; set; }

        public int MutantId { get; set; }
    }
}