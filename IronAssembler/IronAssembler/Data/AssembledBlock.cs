using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class AssembledBlock
	{
		private List<AssembledInstruction> instructions;

		public string Name { get; }
		public IReadOnlyList<AssembledInstruction> Instructions => instructions.AsReadOnly();
		public ulong BlockSizeInBytes { get; }

		public AssembledBlock(string name, IEnumerable<AssembledInstruction> instructions,
			ulong blockSizeInBytes)
		{
			Name = name;
			this.instructions = instructions.ToList();
			BlockSizeInBytes = blockSizeInBytes;
		}
	}
}
