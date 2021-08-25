namespace IronAssembler.DisassemblyWindows
{
    public sealed class WindowInstruction
    {
        public ulong Address { get; }
        public string DisassemblyText { get; }
        public string Bytes { get; }
        public int SizeInBytes { get; }
        
        public WindowInstruction(ulong address, string disassemblyText, string bytes, int sizeInBytes)
        {
            Address = address;
            DisassemblyText = disassemblyText;
            Bytes = bytes;
            SizeInBytes = sizeInBytes;
        }

        public override string ToString() =>
            $"0x{Address:X16} {DisassemblyText}";
    }
}
