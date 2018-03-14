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
			// 1. For each WindowInstruction in instructions,
			//	a. Check if the instruction's address matches the requested address. Return it if so.
			// 2. Throw an exception if no instruction's address matches.

			return null;
		}

		public void PrependInstruction(WindowInstruction instruction)
		{
			// 1. If the instruction occurs IMMEDIATELY before the instruction at index 0,
			//	a. Add the instruction to the list by using InsertAt with index 0.
			//	b. Update StartAddress with the address of the new instruction.
			// 2. If not, throw an exception.
		}

		public void AppendInstruction(WindowInstruction instruction)
		{ 
			// 1. If the instruction occurs IMMEDIATELY after the last instruction in the list,
			//	a. Add the instruction to the list by using Add.
			//	b. Update EndAddress with the address of the end of the new instruction.
			// 2. If not, throw an exception.
		}

		public static InstructionBlock StartNewBlock(WindowInstruction firstInstruction)
		{
			var block = new InstructionBlock();
			block.instructions = new List<WindowInstruction>() { firstInstruction };
			block.StartAddress = firstInstruction.Address;
			block.EndAddress = firstInstruction.Address + (ulong)firstInstruction.SizeInBytes;

			return block;
		}

		public InstructionBlock MergeWith(InstructionBlock other)
		{
			// 1. If other's EndAddress does not equal this block's StartAddress, or vice versa, throw.
			// 2. Create a new InstructionBlock with the two lists of instructions.
			// 3. If other occurs before this, set the new block's StartAddress to other.StartAddress
			//		and the new block's EndAddress to this.EndAddress.
			// 4. If other occurs after this, set the new block's StartAddress to this.StartAddress
			//		and the new block's EndAddress to other.EndAddress.
			// 5. Return the new block.

			return null;
		}
	}
}
