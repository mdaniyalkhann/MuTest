using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MuTest.Core.Model
{
    public class NodeLocation
    {
        public string FilePath { get; set; }

        public IList<Location> Locations { get; } = new List<Location>();
    }
}
