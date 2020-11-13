using System.Collections.Generic;

#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class Project
    {
        private ICollection<Config> _configs;

        public long Id { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public long Shared { get; set; }

        public virtual ICollection<Config> Configs
        {
            get => _configs ?? new HashSet<Config>();
            set => _configs = value;
        }
    }
}
