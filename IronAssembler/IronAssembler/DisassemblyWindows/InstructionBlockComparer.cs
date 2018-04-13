using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.DisassemblyWindows
{
	internal sealed class InstructionBlockComparer : IComparer<InstructionBlock>
	{
		public int Compare(InstructionBlock x, InstructionBlock y)
		{
			if (x.StartAddress > y.EndAddress) { return 1; }
			else if (y.EndAddress < x.StartAddress) { return -1; }
			return 0;
		}
	}
}
