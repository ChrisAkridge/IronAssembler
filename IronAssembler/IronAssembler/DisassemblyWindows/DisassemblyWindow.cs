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
		private bool disposed;
		private int sizeInInstructions;

		public ulong TopAddress { get; private set; }
		private long Position => memoryReader.BaseStream.Position;

		public event EventHandler<EventArgs> InstructionsChanged;

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

		public void SeekToAddress(ulong address)
		{
			memoryReader.BaseStream.Seek((long)address, SeekOrigin.Begin);	

			WindowInstruction[] instructions = new WindowInstruction[SizeInInstructions];
			for (int i = 0; i < sizeInInstructions; i++)
			{
				instructions[i] = DisassembleInstructionAtAddress((ulong)Position);
			}

			this.instructions = instructions;

			OnInstructionsChanged();
		}

		private WindowInstruction DisassembleInstructionAtAddress(ulong address)
		{
			memoryReader.BaseStream.Seek((long)address, SeekOrigin.Begin);
			string disassemblyText = Disassembler.DisassembleInstruction(memoryReader,
				out int instructionLength, out string instructionBytes);
			return new WindowInstruction(address, disassemblyText, instructionBytes,
				instructionLength);
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
