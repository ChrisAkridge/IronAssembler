using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
    public sealed class ParsedStringTable
    {
        private readonly List<string> strings;

        public IReadOnlyList<string> Strings => strings.AsReadOnly();

        public ParsedStringTable(IEnumerable<string> strings) => this.strings = strings.ToList();
    }
}
