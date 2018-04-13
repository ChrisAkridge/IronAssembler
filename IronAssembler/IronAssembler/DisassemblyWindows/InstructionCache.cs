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

		public InstructionCache()
		{
			instructionCache = new SortedSet<InstructionBlock>(new InstructionBlockComparer());
		}

		public bool TryGetInstructionAtAddress(ulong address, out WindowInstruction instruction)
		{
			foreach (InstructionBlock block in instructionCache)
			{
				if (address >= block.StartAddress && address < block.EndAddress)
				{
					instruction = block.GetInstructionAtAddress(address);
					return true;
				}
			}

			instruction = null;
			return false;
		}

		public void CacheInstruction(WindowInstruction instruction)
		{
			ulong instructionStartAddress = instruction.Address;
			ulong instructionEndAddress = instruction.Address + (ulong)instruction.SizeInBytes;

			SortedSet<InstructionBlock>.Enumerator enumerator = instructionCache.GetEnumerator();
			InstructionBlock previousBlock = null;
			while (enumerator.MoveNext())
			{
				InstructionBlock current = enumerator.Current;
				if (current.StartAddress == instructionEndAddress)
				{
					// The instruction can be prepended to the block.
					current.PrependInstruction(instruction);
					if (previousBlock != null) { MergeIfRequired(previousBlock, current); }
					return;
				}
				else if (current.EndAddress == instructionStartAddress)
				{
					// The instruction can be appended to the block.
					current.AppendInstruction(instruction);
					if (enumerator.MoveNext())
					{
						InstructionBlock nextBlock = enumerator.Current;
						MergeIfRequired(current, nextBlock);
					}
					return;
				}

				previousBlock = current;
			}

			InstructionBlock newBlock = InstructionBlock.StartNewBlock(instruction);
			instructionCache.Add(newBlock);
		}

		private bool TryGetInstructionBlockAtAddress(ulong address, out InstructionBlock block)
		{
			// 1. Search through the instruction cache. Return the first block that has the address
			//	>= the block's StartAddress and < the block's EndAddress.

			block = null;
			return false;
		}

		private void MergeIfRequired(InstructionBlock first, InstructionBlock second)
		{
			if (first.EndAddress != second.StartAddress) { return; }

			instructionCache.Remove(first);
			instructionCache.Remove(second);

			InstructionBlock mergedBlock = first.MergeWith(second);
			instructionCache.Add(mergedBlock);
		}
	}
}
