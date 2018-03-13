using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.DisassemblyWindows
{
	internal sealed class InstructionCache
	{
		private SortedSet<InstructionBlock> instructionCache;

		public bool TryGetInstructionAtAddress(ulong address, out WindowInstruction instruction)
		{
			instruction = null;
			return false;
		}

		public void CacheInstruction(WindowInstruction instruction)
		{

		}
	}
}
