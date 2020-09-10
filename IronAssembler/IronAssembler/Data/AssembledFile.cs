using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
    /// <summary>
    /// Represents an assembled file made of assembled blocks..
    /// </summary>
    internal sealed class AssembledFile
    {
        private readonly List<AssembledBlock> blocks;

        /// <summary>
        /// Gets a read-only list of assembled blocks.
        /// </summary>
        public IReadOnlyList<AssembledBlock> Blocks => blocks.AsReadOnly();

        public int SizeOfGlobalVariableBlock { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssembledFile"/> class.
        /// </summary>
        /// <param name="blocks">A sequence of assembled blocks.</param>
        /// <param name="sizeOfGlobalVariableBlock">The size of the global variable block, in bytes.</param>
        public AssembledFile(IEnumerable<AssembledBlock> blocks, int sizeOfGlobalVariableBlock)
        {
            this.blocks = blocks.ToList();
            SizeOfGlobalVariableBlock = sizeOfGlobalVariableBlock;
        }
    }
}
