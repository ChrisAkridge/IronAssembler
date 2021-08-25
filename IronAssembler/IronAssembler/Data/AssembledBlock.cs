using System.Collections.Generic;
using System.Linq;

namespace IronAssembler.Data
{
    /// <summary>
    /// Represents an assembled block containing assembled instructions.
    /// </summary>
    internal sealed class AssembledBlock
    {
        private readonly List<AssembledInstruction> instructions;

        /// <summary>
        /// Gets the name of the block.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a read-only list of assembled instructions within the block.
        /// </summary>
        public IReadOnlyList<AssembledInstruction> Instructions => instructions.AsReadOnly();

        /// <summary>
        /// Gets the size of this block in bytes.
        /// </summary>
        public ulong BlockSizeInBytes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssembledBlock"/> class.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="instructions">A sequence of assembled instructions.</param>
        /// <param name="blockSizeInBytes">The size of the assembled block in bytes.</param>
        public AssembledBlock(string name, IEnumerable<AssembledInstruction> instructions,
            ulong blockSizeInBytes)
        {
            Name = name;
            this.instructions = instructions.ToList();
            BlockSizeInBytes = blockSizeInBytes;
        }
    }
}
