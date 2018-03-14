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
			// 1. Get the block the instruction's in. Return false if there is no block for that
			// address.
			// 2. Call the block's GetInstructionAtAddress method to get the actual instruction.

			instruction = null;
			return false;
		}

		public void CacheInstruction(WindowInstruction instruction)
		{
			// The first question to answer is: Is there a pre-existing block this instruction can
			//	be prepended/appended to? To answer it:
			//	1. Scan through all existing blocks. The block can be used if:
			//		- the instruction's StartAddress + the instruction's length = the block's StartAddress
			//		- the instruction's StartAddress = the block's EndAddress
			// **Be sure to use an enumerator explicitly as we'll need to grab the previous and next
			// blocks to check for merging. Also, keep track of the previous block every time we
			// call MoveNext().**
			
			// If a block can be used:
			//	1. Call PrependInstruction if the instruction's StartAddress < the block's
			//		StartAddress, or AppendInstruction if the instruction's StartAddress = the
			//		block's end address.
			//  2. If the instruction was prepended, call MergeIfRequired(previousBlock, block).
			//		Otherwise, call MergeIfRequired(block, nextBlock).

			// If a block cannot be used:
			//	1. Create a new block with the InstructionBlock.StartNewBlock method.
			//	2. Insert it into the sorted set of blocks.
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
			// First, check if a merge is required:
			//	1. If first's EndAddress = second's StartAddress, a merge is required.
			
			// To perform the merge:
			//	1. Remove both blocks from the cache for now.
			//	2. Call first.MergeWith(second) to get the merged block.
			//	3. Add the merged block to the cache.
		}
	}
}
