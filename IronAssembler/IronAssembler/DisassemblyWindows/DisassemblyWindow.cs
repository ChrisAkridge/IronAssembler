using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.DisassemblyWindows
{
	public sealed class DisassemblyWindow : IDisposable
	{
		private UnmanagedMemoryStream memory;
		private BinaryReader memoryReader;
		private WindowInstruction[] instructions;
		private SortedSet<ulong> knownGoodAddress;
		private InstructionCache cache;

		private bool disposed;

		private bool cachingEnabled;
		private int sizeInInstructions;

		public ulong StartAddress { get; private set; }
		private long Position => memoryReader.BaseStream.Position;

		public event EventHandler<EventArgs> InstructionsChanged;

		public bool CachingEnabled
		{
			get => cachingEnabled;
			set
			{
				// If we're enable caching,
				//	1. Instantiate a new instance of InstructionCache and cache it.
				//	2. For i from 0 to <window size>, cache the instruction returned by GetInstructionAtWindowPosition.
				// If we're disabling caching,
				//	1. Set the cache to null.
				if (value)
				{
					cache = new InstructionCache();
					for (int i = 0; i < SizeInInstructions; i++)
					{
						cache.CacheInstruction(GetInstructionAtWindowPosition(i));
					}
				}
				else { cache = null; }

				cachingEnabled = value;
			}
		}

		public int SizeInInstructions
		{
			get => sizeInInstructions;
			set
			{
				// If the new size is the same as the old size, do nothing.
				// If the new size is larger than the old size:
				//	1. Get the instruction at the (old) last window position. Get its end address by adding address + size in bytes.
				//	2. Create a new instructions array of the new size. Copy over all current instructions.
				//	3. Fill the empty spaces by disassembling each successive instruction.
				//	4. Set the instructions array to the new array.
				// If the new size is smaller than the old size:
				//	1. Create a new instructions array of the new size. Copy over as many current instructions as will fit.
				//	2. Set the instructions array to the new list.

				sizeInInstructions = value;
			}
		}

		public DisassemblyWindow(UnmanagedMemoryStream memory, int sizeInInstructions)
		{
			cachingEnabled = true;
			cache = new InstructionCache();

			this.memory = memory;
			this.sizeInInstructions = sizeInInstructions;

			memoryReader = new BinaryReader(memory);

			instructions = new WindowInstruction[sizeInInstructions];

			for (int i = 0; i < sizeInInstructions; i++)
			{
				instructions[i] = DisassembleInstructionAtAddress((ulong)Position);
			}

			OnInstructionsChanged();
		}

		public WindowInstruction GetInstructionAtWindowPosition(int position) => instructions[position];

		public void Scroll(int distance)
		{
			if (distance < 0)
			{
				for (int i = 0; i < -distance; i++) { ScrollUpOne(); }
			}
			else
			{
				for (int i = 0; i < distance; i++) { ScrollDownOne(); }
			}
		}

		public void ScrollUpOne()
		{
			// Given TOP as the instruction currently at the top of the window,
			// 1. If caching is enabled, look up TOP in the instruction cache.
			//	a. If TOP is found, check to see if there's another instruction before that.
			//		i. If there is another instruction, load it from the cache and go to step 5.
			//		ii. If there isn't another instruction, go to step 2.
			//	b. If TOP is not found, go to step 2.
			// 2. Use FindNearestGoodAddress to find the address that is closest to the TOP's address without going over.
			//	a. If there is no nearest good address, set a new WindowInstruction as follows then go to step 5:
			//		Address: TOP's address - 2
			//		DisassemblyText: ?? ??
			//		Bytes: The last two bytes before TOP.
			//		SizeInBytes: 2
			// 3. From there, call DisassembleInstructionAtAddress in a loop until we reach the address of TOP.
			//	Hold onto the most recently disassembled instruction.
			// 4. Once TOP's address is reached, go to step 5.
			// 5. Create a new array of instructions of the same window size. Copy from index 1 to index [window size - 2].
			// 6. Set the instruction at index 0 to the newly loaded instruction.

			ulong addressOfTopInstruction = GetInstructionAtWindowPosition(0).Address;
			// WYLO: working here on the ScrollUpOne() method. This is where known good address come
			// into play; I think StartAddress is supposed to be what Position is acting as, so we
			// need to switch use of Position to StartAddress (the address of the first window
			// instruction).
		}

		public void ScrollDownOne()
		{
			// Given TOP as the instruction currently at the top of the window,
			//	1. Get the instruction at the last window position. Take its address and add the size in bytes.
			//		This is the address of the new instruction.
			//	2. Disassemble the instruction at this address.
			//	3. Create a new array of instructions and copy from indices 1 to [window size - 1] to indices 0 to [window size - 2].
			//	4. Add the newly disassembled instruction to the last position.
		}

		public void SeekToAddress(ulong address)
		{
			// 1. Create an empty array of instructions.
			// 2. Disassemble enough instructions to fill the window.
			// 3. If the first instruction is valid, add the address parameter as a known good address.
		}

		private WindowInstruction DisassembleInstructionAtAddress(ulong address)
		{
			if (CachingEnabled && cache.TryGetInstructionAtAddress(address, 
				out WindowInstruction cachedInstruction))
			{
				return cachedInstruction;
			}

			memoryReader.BaseStream.Seek((long)address, SeekOrigin.Begin);
			string disassemblyText = Disassembler.DisassembleInstruction(memoryReader,
				out int instructionLength, out string instructionBytes);
			var instruction = new WindowInstruction(address, disassemblyText, instructionBytes,
				instructionLength);
			
			if (CachingEnabled)
			{
				cache.CacheInstruction(instruction);
			}

			return instruction;
		}

		private void FillWindow()
		{
			// 1. If the first instruction slot in the array is null,
			//	a. Use FindNearestGoodAddress to find the address closest to the current start address.
			//	b. If there is no nearest good address, give up and fill the window with ?? ??.
			//	c. If there is, begin disassembling instructions from the known good address up to
			//		and including the start address.
			// 2. Once the first slot is filled, scan through the rest of the array, filling nulls
			//	as we go with newly disassembled instructions.

			if (instructions[0] == null)
			{
				
			}

			OnInstructionsChanged();
		}

		private bool TryFindNearestGoodAddress(ulong address, out ulong goodAddress)
		{
			// Given our sorted set of known good addresses, go through it until we find the one
			//	closest to the provided address without going over.

			goodAddress = 0UL;
			return false;
		}

		private void OnInstructionsChanged()
		{
			InstructionsChanged?.Invoke(this, new EventArgs());
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					memoryReader.Dispose();
					// don't dispose memory here since it will dispose the unmanaged pointer
					// which is probably still in use by the IronArc VM
				}
				disposed = true;
			}
		}
	}
}
