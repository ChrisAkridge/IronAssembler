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

		public AssembledFile(IEnumerable<AssembledBlock> blocks)
		{
			this.blocks = blocks.ToList();
		}
	}
}
