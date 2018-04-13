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

		private InstructionBlock() { }

		public WindowInstruction GetInstructionAtAddress(ulong address)
		{
			foreach (WindowInstruction instruction in instructions)
			{
				if (instruction.Address == address) { return instruction; }
			}

			throw new ArgumentException($"No instruction at address 0x{address:X16} exists; are you sure you're in the right block?",
				nameof(address));
		}

		public void PrependInstruction(WindowInstruction instruction)
		{
			if ((instruction.Address + (ulong)instruction.SizeInBytes) != instructions[0].Address)
			{
				throw new ArgumentException($"The instruction at 0x{instruction.Address:X16} (size 0x{instruction.SizeInBytes:X8}) isn't immediately before the first instruction in this block, which is at address 0x{instructions[0].Address:X16}.",
					nameof(instruction));
			}

			instructions.Insert(0, instruction);
			StartAddress = instruction.Address;
		}

		public void AppendInstruction(WindowInstruction instruction)
		{
			WindowInstruction lastInstruction = instructions.Last();
			if ((lastInstruction.Address + (ulong)lastInstruction.SizeInBytes) != instruction.Address)
			{
				throw new ArgumentException($"The instruction at 0x{instruction.Address:X16} isn't immediately after the last instruction in this block, which is at address 0x{lastInstruction.Address:X16} (size 0x{lastInstruction.SizeInBytes:X8}).",
					nameof(instruction));
			}

			instructions.Add(instruction);
			EndAddress = instruction.Address + (ulong)instruction.SizeInBytes;
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
			if (other.EndAddress != StartAddress || other.StartAddress != EndAddress)
			{
				throw new ArgumentException($"The instruction blocks to merge were not adjacent. Block 1 starts at 0x{StartAddress:X16}, ends at 0x{EndAddress:X16}. Block 2 starts at 0x{other.StartAddress:X16}, ends at 0x{other.EndAddress:X16}.",
					nameof(other));
			}

			var mergedBlock = new InstructionBlock();

			if (other.EndAddress == StartAddress)
			{
				// other is BEFORE this block
				mergedBlock.instructions = new List<WindowInstruction>(other.instructions.Concat(instructions));
				mergedBlock.StartAddress = other.StartAddress;
				mergedBlock.EndAddress = EndAddress;
			}
			else if (EndAddress == other.StartAddress)
			{
				// other is AFTER this block
				mergedBlock.instructions = new List<WindowInstruction>(instructions.Concat(other.instructions));
				mergedBlock.StartAddress = StartAddress;
				mergedBlock.EndAddress = other.EndAddress;
			}

			return mergedBlock;
		}
	}
}
