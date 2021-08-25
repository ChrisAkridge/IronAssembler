namespace IronAssembler.Data
{
    /// <summary>
    /// Represents the information for an IronArc instruction.
    /// </summary>
    internal sealed class InstructionInfo
    {
        /// <summary>
        /// Gets the proper name of this instruction, suitable for display.
        /// </summary>
        public string ProperName { get; }

        /// <summary>
        /// Gets the mnemonic of this instruction, used in assembly files.
        /// </summary>
        public string Mnemonic { get; }

        /// <summary>
        /// Gets the two-byte opcode of this instruction, used in executable files.
        /// </summary>
        public ushort Opcode { get; }

        /// <summary>
        /// Gets a value indicating whether this instruction needs a flags byte to follow the opcode.
        /// </summary>
        public bool NeedsFlags { get; }

        /// <summary>
        /// Gets a value indicating whether this instruction needs a defined operand size.
        /// </summary>
        public bool NeedsSize { get; }

        /// <summary>
        /// Gets a value indicating how many operands (0 to 3) this instruction needs.
        /// </summary>
        public int OperandCount { get; }

        /// <summary>
        /// Gets a value indicating whether this instruction can use labels as operands.
        /// </summary>
        public bool CanUseLabels { get; }
        
        public OperandSize?[] ImplicitSizes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionInfo"/> class.
        /// </summary>
        /// <param name="properName">The proper name of this instruction, suitable for display.</param>
        /// <param name="mnemonic">The mnemonic of this instruction, used in assembly files.</param>
        /// <param name="opcode">The two-byte opcode of this instruction, used in executable files.</param>
        /// <param name="needsFlags">A value indicating whether this instruction needs a flags byte to follow the opcode.</param>
        /// <param name="needsSize">A value indicating whether this instruction needs a defined operand size.</param>
        /// <param name="operandCount">A value indicating how many operands (0 to 3) this instruction needs.</param>
        /// <param name="canUseLabels">A value indicating whether this instruction can use labels as operands.</param>
        /// <param name="implicitSizes">An optional array that specifies the implicit sizes of the operands. Set to null if no operand have implicit sizes, set array elements to null if that operand has no implicit size.</param>
        public InstructionInfo(string properName, string mnemonic, ushort opcode, bool needsFlags,
            bool needsSize, int operandCount, bool canUseLabels, OperandSize?[] implicitSizes = null)
        {
            ProperName = properName;
            Mnemonic = mnemonic;
            Opcode = opcode;
            NeedsFlags = needsFlags;
            NeedsSize = needsSize;
            OperandCount = operandCount;
            CanUseLabels = canUseLabels;
            ImplicitSizes = implicitSizes ?? new OperandSize?[operandCount];
        }

        public override string ToString() => Mnemonic;
    }
}
