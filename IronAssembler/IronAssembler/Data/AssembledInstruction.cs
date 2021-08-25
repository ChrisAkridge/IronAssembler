﻿using System.Collections.Generic;
using System.Linq;

namespace IronAssembler.Data
{
    /// <summary>
    /// Represents an assembled instruction.
    /// </summary>
    internal sealed class AssembledInstruction
    {
        private readonly List<byte> bytes;

        /// <summary>
        /// Gets a read-only list of bytes that make up this instruction.
        /// </summary>
        public IReadOnlyList<byte> Bytes => bytes.AsReadOnly();

        /// <summary>
        /// Gets the label of the first operand, if the first operand has a label, or null if otherwise.
        /// </summary>
        public string Operand1Label { get; }

        /// <summary>
        /// Gets the label of the second operand, if the second operand has a label, or null if otherwise.
        /// </summary>
        public string Operand2Label { get; }

        /// <summary>
        /// Gets the label of the third operand, if the third operand has a label, or null if otherwise.
        /// </summary>
        public string Operand3Label { get; }
        
        public int Operand1StringIndex { get; }
        public int Operand2StringIndex { get; }
        public int Operand3StringIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssembledInstruction"/> class.
        /// </summary>
        /// <param name="bytes">A sequence of bytes that makes up the instruction.</param>
        /// <param name="operand1Label">The label of the first operand.</param>
        /// <param name="operand2Label">The label of the second operand.</param>
        /// <param name="operand3Label">The label of the third operand.</param>
        /// <param name="operand1StringIndex">The string table index of the first operand, or -1 if this operand is not a string table index.</param>
        /// <param name="operand2StringIndex">The string table index of the second operand, or -1 if this operand is not a string table index.</param>
        /// <param name="operand3StringIndex">The string table index of the third operand, or -1 if this operand is not a string table index.</param>
        public AssembledInstruction(IEnumerable<byte> bytes,
            string operand1Label = null,
            string operand2Label = null,
            string operand3Label = null,
            int operand1StringIndex = -1,
            int operand2StringIndex = -1,
            int operand3StringIndex = -1)
        {
            this.bytes = bytes.ToList();
            Operand1Label = operand1Label;
            Operand2Label = operand2Label;
            Operand3Label = operand3Label;
            Operand1StringIndex = operand1StringIndex;
            Operand2StringIndex = operand2StringIndex;
            Operand3StringIndex = operand3StringIndex;
        }
    }
}
