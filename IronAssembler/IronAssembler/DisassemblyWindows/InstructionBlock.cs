using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.DisassemblyWindows
{
	internal sealed class InstructionBlock
	{
		private List<WindowInstruction> instructions = new List<WindowInstruction>();
	
		public ulong StartAddress { get; private set; }
		public ulong EndAddress { get; private set; }

		public WindowInstruction GetInstructionAtAddress(ulong address)
		{
			return null;
		}

		public void PrependInstruction(WindowInstruction instruction)
		{

		}

		public void AppendInstruction(WindowInstruction instruction)
		{ 
		
		}

		public void MergeWith(InstructionBlock other)
		{

		}
	}
}
