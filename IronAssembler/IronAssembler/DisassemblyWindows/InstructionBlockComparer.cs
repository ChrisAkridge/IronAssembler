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
			// 1. If a's start is after b's end, a > b (return +1).
			// 2. If b's end if before a's start, a < b (return -1).
			// 3. Otherwise, either a and b are over the same addresses, or their is overlap. a == b (return 0).

			return 0;
		}
	}
}
