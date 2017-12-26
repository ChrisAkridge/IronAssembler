using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class ParsedFile
	{
		private List<ParsedBlock> blocks;

		public IReadOnlyList<ParsedBlock> Blocks => blocks.AsReadOnly();
		public ParsedStringTable StringTable { get; }

		public ParsedFile(IEnumerable<ParsedBlock> blocks, ParsedStringTable stringTable)
		{
			this.blocks = blocks.ToList();
			StringTable = stringTable;
		}
	}
}
