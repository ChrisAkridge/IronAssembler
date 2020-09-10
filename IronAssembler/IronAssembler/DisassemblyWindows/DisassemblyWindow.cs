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
        private readonly UnmanagedMemoryStream memory;
        private readonly BinaryReader memoryReader;
        private WindowInstruction[] instructions;
        private bool disposed;

        private long Position => memoryReader.BaseStream.Position;

        public event EventHandler<EventArgs> InstructionsChanged;

        public int SizeInInstructions { get; set; }

        public DisassemblyWindow(UnmanagedMemoryStream memory, int sizeInInstructions)
        {
            this.memory = memory;
            SizeInInstructions = sizeInInstructions;

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

            var windowInstructions = new WindowInstruction[SizeInInstructions];
            for (int i = 0; i < SizeInInstructions; i++)
            {
                windowInstructions[i] = DisassembleInstructionAtAddress((ulong)Position);
            }

            this.instructions = windowInstructions;

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

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposed) { return; }

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
