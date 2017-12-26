using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class AssembledFile
	{
		private List<AssembledBlock> blocks;

		public IReadOnlyList<AssembledBlock> Blocks => blocks.AsReadOnly();
		public AssembledStringsTable StringsTable { get; }

		public AssembledFile(IEnumerable<AssembledBlock> blocks, AssembledStringsTable stringsTable)
		{
			this.blocks = blocks.ToList();
			StringsTable = stringsTable;
		}
	}
}
