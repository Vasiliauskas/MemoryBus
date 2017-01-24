using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryBus.Tests
{
    class ContractA
    {
        public string Name { get; set; }
    }

    class ContractB
    {
        public Guid Id { get; set; }
        public List<string> Members { get; set; }
    }
}
